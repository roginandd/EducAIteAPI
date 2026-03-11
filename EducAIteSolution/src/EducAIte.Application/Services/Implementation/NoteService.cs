using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<NoteService> _logger;

    public NoteService(
        INoteRepository noteRepository,
        IDocumentRepository documentRepository,
        ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<NoteResponse?> GetNoteByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        Note? note = await _noteRepository.GetByExternalIdAsync(externalId, cancellationToken);

        return note?.ToResponse();
    }

    public async Task<IEnumerable<NoteResponse>> GetNotesByDocumentAsync(long documentId, CancellationToken cancellationToken = default)
    {
        await EnsureDocumentExistsAsync(documentId, cancellationToken);

        IReadOnlyList<Note> notes = await _noteRepository.GetAllByDocumentIdAsync(documentId, cancellationToken);
        return notes.Select(note => note.ToResponse());
    }

    public async Task<NoteResponse> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        Note note = request.ToEntity();
        await EnsureDocumentExistsAsync(note.DocumentId, cancellationToken);

        Note createdNote = await _noteRepository.AddAsync(note, cancellationToken);
        _logger.LogInformation("Created note {NoteId}", createdNote.NoteId);

        return createdNote.ToResponse();
    }

    public async Task<bool> UpdateNoteAsync(Guid externalId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        Note? existingNote = await _noteRepository.GetTrackedByExternalIdAsync(externalId, cancellationToken);
        if (existingNote is null)
        {
            return false;
        }

        if (IsUnchanged(existingNote, request))
        {
            return true;
        }

        await EnsureDocumentExistsAsync(request.DocumentId, cancellationToken);

        request.ApplyToEntity(existingNote);
        await _noteRepository.UpdateAsync(existingNote, cancellationToken);

        _logger.LogInformation("Updated note {ExternalId}", externalId);
        return true;
    }

    public async Task<bool> PatchNoteAsync(Guid externalId, PatchNoteRequest request, CancellationToken cancellationToken = default)
    {
        if (IsPatchEmpty(request))
        {
            throw new ArgumentException("At least one field must be provided.");
        }

        Note? existingNote = await _noteRepository.GetTrackedByExternalIdAsync(externalId, cancellationToken);
        if (existingNote is null)
        {
            return false;
        }

        if (request.Name is not null)
        {
            existingNote.Rename(request.Name);
        }

        if (request.NoteContent is not null)
        {
            existingNote.UpdateContent(request.NoteContent);
        }

        if (request.DocumentId.HasValue)
        {
            await EnsureDocumentExistsAsync(request.DocumentId.Value, cancellationToken);
            existingNote.MoveToDocument(request.DocumentId.Value);
        }

        if (request.SequenceNumber.HasValue)
        {
            existingNote.UpdateSequenceNumber(request.SequenceNumber.Value);
        }

        await _noteRepository.UpdateAsync(existingNote, cancellationToken);
        _logger.LogInformation("Patched note {ExternalId}", externalId);

        return true;
    }

    public async Task<bool> DeleteNoteAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        bool isDeleted = await _noteRepository.SoftDeleteByExternalIdAsync(externalId, cancellationToken);
        if (isDeleted)
        {
            _logger.LogInformation("Deleted note {ExternalId}", externalId);
        }

        return isDeleted;
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

    private static bool IsUnchanged(Note existingNote, UpdateNoteRequest request)
    {
        return string.Equals(existingNote.Name, request.Name.Trim(), StringComparison.Ordinal) &&
               string.Equals(existingNote.NoteContent, request.NoteContent.Trim(), StringComparison.Ordinal) &&
               existingNote.DocumentId == request.DocumentId &&
               existingNote.SequenceNumber == request.SequenceNumber;
    }

    private static bool IsPatchEmpty(PatchNoteRequest request)
    {
        return request.Name is null &&
               request.NoteContent is null &&
               request.DocumentId is null &&
               request.SequenceNumber is null;
    }
}
