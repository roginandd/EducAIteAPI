using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.Document;
using EducAIte.Domain.Exceptions.Flashcard;
using EducAIte.Domain.Exceptions.Note;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly ISqidService _sqidService;
    private readonly ILogger<FlashcardService> _logger;

    public FlashcardService(
        IFlashcardRepository flashcardRepository,
        IDocumentRepository documentRepository,
        INoteRepository noteRepository,
        IStudentFlashcardRepository studentFlashcardRepository,
        IResourceOwnershipService resourceOwnershipService,
        ISqidService sqidService,
        ILogger<FlashcardService> logger)
    {
        _flashcardRepository = flashcardRepository;
        _documentRepository = documentRepository;
        _noteRepository = noteRepository;
        _studentFlashcardRepository = studentFlashcardRepository;
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
            throw new InvalidSqidException(nameof(flashcardSqid));
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
            throw new InvalidSqidException(nameof(documentSqid));
        }

        await EnsureDocumentOwnedAndExistsAsync(documentId, studentId, cancellationToken);
        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByDocumentIdAndStudentIdAsync(documentId, studentId, cancellationToken);
        return flashcards.Select(f => f.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<FlashcardResponse>> GetByNoteAsync(
        string noteSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        long noteId = DecodeRequiredSqid(noteSqid, "NoteSqid");
        await EnsureNoteOwnedAndExistsAsync(noteId, studentId, cancellationToken);

        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByNoteIdAndStudentIdAsync(noteId, studentId, cancellationToken);
        return flashcards.Select(f => f.ToResponse(_sqidService)).ToList();
    }

    public async Task<FlashcardResponse> CreateAsync(
        CreateFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        long noteId = DecodeRequiredSqid(request.NoteSqid, "NoteSqid");

        await EnsureNoteOwnedAndExistsAsync(noteId, studentId, cancellationToken);

        Flashcard flashcard = request.ToEntity(noteId);
        Note? targetNote = await _noteRepository.GetTrackedByIdAsync(noteId, cancellationToken);
        if (targetNote is null)
        {
            throw new NoteNotFoundException(noteId);
        }

        targetNote.AddFlashcard(flashcard);

        Flashcard created = await _flashcardRepository.AddAsync(flashcard, cancellationToken);
        _logger.LogInformation("Created flashcard {FlashcardId}", created.FlashcardId);

        return created.ToResponse(_sqidService);
    }

    public async Task<IReadOnlyList<FlashcardResponse>> CreateBulkAsync(
        CreateBulkFlashcardsRequest request,
        long studentId,
        CancellationToken cancellationToken = default
        )
    {
        EnsureStudentIdIsValid(studentId);

        long noteId = DecodeRequiredSqid(request.Notesqid, "NoteSqid");

        await EnsureNoteOwnedAndExistsAsync(noteId, studentId, cancellationToken);

        Note? targetNote = await _noteRepository.GetTrackedByIdAsync(noteId, cancellationToken);
        if (targetNote is null)
        {
            throw new NoteNotFoundException(noteId);
        }

        List<Flashcard> flashcardsToCreate = request.Flashcards
            .Select(fc => fc.ToEntity(noteId))
            .ToList();
        
        targetNote.AddFlashcards(flashcardsToCreate);

        await _flashcardRepository.AddRangeAsync(flashcardsToCreate, cancellationToken);
        _logger.LogInformation("Created {Count} flashcards for note {NoteId}", flashcardsToCreate.Count, noteId);

        return flashcardsToCreate.Select(fc => fc.ToResponse(_sqidService)).ToList();
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
            throw new InvalidSqidException(nameof(flashcardSqid));
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

        long noteId = DecodeRequiredSqid(request.NoteSqid, "NoteSqid");

        await EnsureNoteOwnedAndExistsAsync(noteId, studentId, cancellationToken);

        request.UpdateFromEntity(existing);

        if (existing.NoteId != noteId)
        {
            Note? targetNote = await _noteRepository.GetTrackedByIdAsync(noteId, cancellationToken);
            if (targetNote is null)
            {
                throw new NoteNotFoundException(noteId);
            }

            targetNote.ReassignFlashcard(existing);
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
            throw new InvalidSqidException(nameof(flashcardSqid));
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

        IReadOnlyList<StudentFlashcard> progressEntries = await _studentFlashcardRepository.GetTrackedByFlashcardIdAsync(flashcardId, cancellationToken);
        existing.MarkDeletedWithProgress(progressEntries);
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
            throw new FlashcardForbiddenException();
        }

        throw new FlashcardNotFoundException(flashcardId);
    }

    private async Task EnsureDocumentOwnedAndExistsAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken)
    {
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(documentId);
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
    }

    private async Task EnsureNoteOwnedAndExistsAsync(
        long noteId,
        long studentId,
        CancellationToken cancellationToken)
    {
        Note? note = await _noteRepository.GetByIdAsync(noteId, cancellationToken);
        if (note is null)
        {
            throw new NoteNotFoundException(noteId);
        }

        await _resourceOwnershipService.EnsureNoteOwnedByStudentAsync(noteId, studentId, cancellationToken);
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new InvalidSqidException(fieldName);
        }

        return id;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new FlashcardValidationException("StudentId must be greater than zero.");
        }
    }
}
