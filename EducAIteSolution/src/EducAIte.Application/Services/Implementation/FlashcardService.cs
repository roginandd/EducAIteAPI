using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly ISqidService _sqidService;
    private readonly ILogger<FlashcardService> _logger;

    public FlashcardService(
        IFlashcardRepository flashcardRepository,
        IDocumentRepository documentRepository,
        IResourceOwnershipService resourceOwnershipService,
        ISqidService sqidService,
        ILogger<FlashcardService> logger)
    {
        _flashcardRepository = flashcardRepository;
        _documentRepository = documentRepository;
        _resourceOwnershipService = resourceOwnershipService;
        _sqidService = sqidService;
        _logger = logger;
    }

    public async Task<FlashcardResponse?> GetBySqidAsync(
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(flashcardSqid, out long flashcardId))
        {
            throw new ArgumentException("FlashcardSqid is invalid.", nameof(flashcardSqid));
        }

        Flashcard? flashcard = await _flashcardRepository.GetByIdAndStudentIdAsync(
            flashcardId,
            studentId,
            cancellationToken);

        if (flashcard is null)
        {
            await EnsureFlashcardOwnedByStudentAsync(flashcardId, studentId, cancellationToken);
            return null;
        }

        return flashcard.ToResponse(_sqidService);
    }

    public async Task<IReadOnlyList<FlashcardResponse>> GetMineAsync(long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        return flashcards.Select(f => f.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<FlashcardResponse>> GetByDocumentAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(documentSqid, out long documentId))
        {
            throw new ArgumentException("DocumentSqid is invalid.", nameof(documentSqid));
        }

        await EnsureDocumentOwnedAndExistsAsync(documentId, studentId, cancellationToken);
        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByDocumentIdAndStudentIdAsync(documentId, studentId, cancellationToken);
        return flashcards.Select(f => f.ToResponse(_sqidService)).ToList();
    }

    public async Task<FlashcardResponse> CreateAsync(
        CreateFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        long documentId = DecodeRequiredSqid(request.DocumentSqid, "DocumentSqid");

        await EnsureDocumentOwnedAndExistsAsync(documentId, studentId, cancellationToken);

        Flashcard flashcard = request.ToEntity(documentId);
        Document? targetDocument = await _documentRepository.GetTrackedByIdAsync(documentId, cancellationToken);
        if (targetDocument is null)
        {
            throw new ArgumentException("DocumentSqid is invalid.", nameof(request.DocumentSqid));
        }

        targetDocument.AddFlashcard(flashcard);

        Flashcard created = await _flashcardRepository.AddAsync(flashcard, cancellationToken);
        _logger.LogInformation("Created flashcard {FlashcardId}", created.FlashcardId);

        return created.ToResponse(_sqidService);
    }

    public async Task<bool> UpdateAsync(
        string flashcardSqid,
        UpdateFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(flashcardSqid, out long flashcardId))
        {
            throw new ArgumentException("FlashcardSqid is invalid.", nameof(flashcardSqid));
        }

        Flashcard? existing = await _flashcardRepository.GetTrackedByIdAndStudentIdAsync(
            flashcardId,
            studentId,
            cancellationToken);

        if (existing is null)
        {
            await EnsureFlashcardOwnedByStudentAsync(flashcardId, studentId, cancellationToken);
            return false;
        }

        long documentId = DecodeRequiredSqid(request.DocumentSqid, "DocumentSqid");

        await EnsureDocumentOwnedAndExistsAsync(documentId, studentId, cancellationToken);

        request.UpdateFromEntity(existing);

        if (existing.DocumentId != documentId)
        {
            Document? targetDocument = await _documentRepository.GetTrackedByIdAsync(documentId, cancellationToken);
            if (targetDocument is null)
            {
                throw new ArgumentException("DocumentSqid is invalid.", nameof(request.DocumentSqid));
            }

            targetDocument.ReassignFlashcard(existing);
        }

        await _flashcardRepository.UpdateAsync(existing, cancellationToken);
        _logger.LogInformation("Updated flashcard {FlashcardSqid}", flashcardSqid);
        return true;
    }

    public async Task<bool> DeleteAsync(
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(flashcardSqid, out long flashcardId))
        {
            throw new ArgumentException("FlashcardSqid is invalid.", nameof(flashcardSqid));
        }

        Flashcard? existing = await _flashcardRepository.GetTrackedByIdAndStudentIdAsync(
            flashcardId,
            studentId,
            cancellationToken);

        if (existing is null)
        {
            await EnsureFlashcardOwnedByStudentAsync(flashcardId, studentId, cancellationToken);
            return false;
        }

        existing.MarkDeleted();
        await _flashcardRepository.UpdateAsync(existing, cancellationToken);
        _logger.LogInformation("Deleted flashcard {FlashcardSqid}", flashcardSqid);

        return true;
    }

    private async Task EnsureFlashcardOwnedByStudentAsync(
        long flashcardId,
        long studentId,
        CancellationToken cancellationToken)
    {
        bool exists = await _flashcardRepository.ExistsByIdAsync(flashcardId, cancellationToken);
        if (exists)
        {
            throw new UnauthorizedAccessException("Flashcard does not belong to the authenticated student.");
        }
    }

    private async Task EnsureDocumentOwnedAndExistsAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken)
    {
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            throw new ArgumentException("DocumentSqid is invalid.", nameof(documentId));
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new ArgumentException($"{fieldName} is invalid.");
        }

        return id;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }
}
