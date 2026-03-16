using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class NoteService : INoteService
{
    private const int MaxUniqueConflictRetries = 3;

    private readonly INoteRepository _noteRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INoteOrderingService _noteOrderingService;
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NoteService> _logger;

    public NoteService(
        INoteRepository noteRepository,
        IDocumentRepository documentRepository,
        INoteOrderingService noteOrderingService,
        IResourceOwnershipService resourceOwnershipService,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _documentRepository = documentRepository;
        _noteOrderingService = noteOrderingService;
        _resourceOwnershipService = resourceOwnershipService;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<NoteResponse?> GetNoteBySqidAsync(
        string sqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!TryDecodeSqid(sqid, out long noteId))
        {
            return null;
        }

        await _resourceOwnershipService.EnsureNoteOwnedByStudentAsync(noteId, studentId, cancellationToken);
        Note? note = await _noteRepository.GetByIdAsync(noteId, cancellationToken);

        return note is null ? null : note.ToResponse(_sqidService);
    }

    public async Task<IReadOnlyList<NoteResponse>> GetNotesByDocumentAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(documentSqid, out long documentId))
        {
            return [];
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        IReadOnlyList<Note> notes = await _noteRepository.GetAllByDocumentIdAndStudentIdAsync(
            documentId,
            studentId,
            cancellationToken);
        return notes.Select(n => n.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<NoteResponse>> GetNotesByStudentAsync(long studentId, CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }

        IReadOnlyList<Note> notes = await _noteRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        return notes.Select(n => n.ToResponse(_sqidService)).ToList();
    }

    public async Task<NoteResponse> CreateNoteAsync(
        CreateNoteRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(request.DocumentSqid, out long documentId))
        {
            throw new ArgumentException("DocumentSqid is invalid.");
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        await EnsureDocumentExistsAsync(documentId, cancellationToken);
        for (int attempt = 0; attempt <= MaxUniqueConflictRetries; attempt++)
        {
            decimal nextSequence = await _noteOrderingService.ComputeAppendLastSequenceAsync(
                request.DocumentSqid,
                cancellationToken);

            Note note = new(request.Name, request.NoteContent, documentId, nextSequence);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _noteRepository.AddAsync(note, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex) when (attempt < MaxUniqueConflictRetries && IsUniqueSequenceConflict(ex))
            {
                await _noteOrderingService.RebalanceAsync(studentId, request.DocumentSqid, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating note for student {StudentId}", studentId);
                throw;
            }

            Note? createdNote = await _noteRepository.GetByIdAsync(note.NoteId, cancellationToken);

            _logger.LogInformation("Created note {NoteId}", note.NoteId);

            if (createdNote is null)
            {
                throw new InvalidOperationException("Note was created but could not be reloaded.");
            }

            return createdNote.ToResponse(_sqidService);
        }

        throw new InvalidOperationException("Unable to create note after retrying due to sequence conflicts.");
    }

    public async Task<bool> UpdateNoteAsync(
        string sqid,
        UpdateNoteRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!TryDecodeSqid(sqid, out long noteId))
        {
            return false;
        }

        await _resourceOwnershipService.EnsureNoteOwnedByStudentAsync(noteId, studentId, cancellationToken);
        Note? existingNote = await _noteRepository.GetTrackedByIdAsync(noteId, cancellationToken);
        if (existingNote is null)
        {
            return false;
        }

        if (!_sqidService.TryDecode(request.DocumentSqid, out long documentId))
        {
            throw new ArgumentException("DocumentSqid is invalid.");
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        if (IsUnchanged(existingNote, request, documentId))
        {
            return true;
        }

        await EnsureDocumentExistsAsync(documentId, cancellationToken);
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            existingNote.UpdateDetails(request.Name, request.NoteContent);

            if (existingNote.DocumentId != documentId)
            {
                Document? targetDocument = await _documentRepository.GetTrackedByIdAsync(documentId, cancellationToken);
                if (targetDocument is null)
                {
                    throw new KeyNotFoundException($"Document with ID {documentId} not found.");
                }

                targetDocument.ReassignNote(existingNote);
            }

            await _noteRepository.UpdateAsync(existingNote, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating note {NoteSqid}", sqid);
            throw;
        }

        _logger.LogInformation("Updated note {NoteSqid}", sqid);
        return true;
    }

    public async Task<bool> PatchNoteAsync(
        string sqid,
        PatchNoteRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (IsPatchEmpty(request))
        {
            throw new ArgumentException("At least one field must be provided.");
        }

        if (!TryDecodeSqid(sqid, out long noteId))
        {
            return false;
        }

        await _resourceOwnershipService.EnsureNoteOwnedByStudentAsync(noteId, studentId, cancellationToken);
        Note? existingNote = await _noteRepository.GetTrackedByIdAsync(noteId, cancellationToken);
        if (existingNote is null)
        {
            return false;
        }

        long? patchedDocumentId = null;
        if (request.DocumentSqid is not null)
        {
            if (!_sqidService.TryDecode(request.DocumentSqid, out long decodedDocumentId))
            {
                throw new ArgumentException("DocumentSqid is invalid.");
            }

            await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(decodedDocumentId, studentId, cancellationToken);
            await EnsureDocumentExistsAsync(decodedDocumentId, cancellationToken);
            patchedDocumentId = decodedDocumentId;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (request.Name is not null)
            {
                existingNote.Rename(request.Name);
            }

            if (request.NoteContent is not null)
            {
                existingNote.UpdateContent(request.NoteContent);
            }

            if (request.DocumentSqid is not null)
            {
                Document? targetDocument = await _documentRepository.GetTrackedByIdAsync(patchedDocumentId!.Value, cancellationToken);
                if (targetDocument is null)
                {
                    throw new KeyNotFoundException($"Document with ID {patchedDocumentId.Value} not found.");
                }

                targetDocument.ReassignNote(existingNote);
            }

            await _noteRepository.UpdateAsync(existingNote, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while patching note {NoteSqid}", sqid);
            throw;
        }

        _logger.LogInformation("Patched note {NoteSqid}", sqid);

        return true;
    }

    public async Task<bool> DeleteNoteAsync(string sqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!TryDecodeSqid(sqid, out long noteId))
        {
            return false;
        }

        await _resourceOwnershipService.EnsureNoteOwnedByStudentAsync(noteId, studentId, cancellationToken);
        Note? existingNote = await _noteRepository.GetByIdAsync(noteId, cancellationToken);
        if (existingNote is null)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _noteRepository.DeleteAsync(noteId, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting note {NoteSqid}", sqid);
            throw;
        }

        _logger.LogInformation("Deleted note {NoteSqid}", sqid);

        return true;
    }

    private async Task EnsureDocumentExistsAsync(long documentId, CancellationToken cancellationToken)
    {
        if (documentId <= 0)
        {
            throw new ArgumentException("DocumentId must be greater than zero.");
        }

        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            throw new KeyNotFoundException($"Document with ID {documentId} not found.");
        }
    }

    private static bool IsUnchanged(Note existingNote, UpdateNoteRequest request, long documentId)
    {
        return string.Equals(existingNote.Name, request.Name.Trim(), StringComparison.Ordinal) &&
               string.Equals(existingNote.NoteContent, request.NoteContent.Trim(), StringComparison.Ordinal) &&
               existingNote.DocumentId == documentId;
    }

    private static bool IsPatchEmpty(PatchNoteRequest request)
    {
        return request.Name is null &&
               request.NoteContent is null &&
               request.DocumentSqid is null;
    }

    private bool TryDecodeSqid(string sqid, out long noteId)
    {
        return _sqidService.TryDecode(sqid, out noteId);
    }


    private static bool IsUniqueSequenceConflict(Exception exception)
    {
        Exception? current = exception;
        while (current is not null)
        {
            if (current.Message.Contains("IX_Notes_DocumentId_SequenceNumber", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }
}
