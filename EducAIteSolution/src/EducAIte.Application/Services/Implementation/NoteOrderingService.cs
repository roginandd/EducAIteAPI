using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;

namespace EducAIte.Application.Services.Implementation;

public sealed class NoteOrderingService : INoteOrderingService
{
    private const decimal DefaultStep = 100m;
    private const decimal MinGapThreshold = 0.000001m;
    private const int MaxUniqueConflictRetries = 3;

    private readonly INoteRepository _noteRepository;
    private readonly ISqidService _sqidService;

    public NoteOrderingService(INoteRepository noteRepository, ISqidService sqidService)
    {
        _noteRepository = noteRepository;
        _sqidService = sqidService;
    }

    public async Task<decimal> ComputeAppendLastSequenceAsync(
        string documentSqid,
        CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(documentSqid, out long documentId))
        {
            throw new ArgumentException("DocumentSqid is invalid.");
        }

        Note? last = await _noteRepository.GetLastByDocumentIdAsync(documentId, cancellationToken);
        return last is null ? DefaultStep : last.SequenceNumber + DefaultStep;
    }

    public async Task<bool> MoveBetweenAsync(
        string documentSqid,
        string noteSqid,
        string? previousNoteSqid,
        string? nextNoteSqid,
        CancellationToken cancellationToken = default)
    {
        if (!TryDecodeDocumentAndNote(documentSqid, noteSqid, out long documentId, out long noteId))
        {
            return false;
        }

        for (int attempt = 0; attempt <= MaxUniqueConflictRetries; attempt++)
        {
            Note? note = await _noteRepository.GetTrackedByIdAndDocumentIdAsync(noteId, documentId, cancellationToken);
            if (note is null)
            {
                return false;
            }

            decimal sequence = await ComputeSequenceAsync(documentId, previousNoteSqid, nextNoteSqid, cancellationToken);
            note.UpdateSequenceNumber(sequence);

            try
            {
                await _noteRepository.UpdateAsync(note, cancellationToken);
                return true;
            }
            catch (Exception ex) when (attempt < MaxUniqueConflictRetries && IsUniqueSequenceConflict(ex))
            {
                await _noteRepository.RebalanceDocumentAsync(documentId, DefaultStep, cancellationToken);
            }
        }

        throw new InvalidOperationException("Unable to reorder note after retrying due to sequence conflicts.");
    }

    public async Task RebalanceAsync(string documentSqid, CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(documentSqid, out long documentId))
        {
            throw new ArgumentException("DocumentSqid is invalid.");
        }

        await _noteRepository.RebalanceDocumentAsync(documentId, DefaultStep, cancellationToken);
    }

    private async Task<decimal> ComputeSequenceAsync(
        long documentId,
        string? previousNoteSqid,
        string? nextNoteSqid,
        CancellationToken cancellationToken)
    {
        if (previousNoteSqid is null && nextNoteSqid is null)
        {
            Note? last = await _noteRepository.GetLastByDocumentIdAsync(documentId, cancellationToken);
            return last is null ? DefaultStep : last.SequenceNumber + DefaultStep;
        }

        if (previousNoteSqid is null && nextNoteSqid is not null)
        {
            Note next = await ResolveNeighborAsync(documentId, nextNoteSqid, cancellationToken);
            return next.SequenceNumber - DefaultStep;
        }

        if (previousNoteSqid is not null && nextNoteSqid is null)
        {
            Note previous = await ResolveNeighborAsync(documentId, previousNoteSqid, cancellationToken);
            return previous.SequenceNumber + DefaultStep;
        }

        Note prev = await ResolveNeighborAsync(documentId, previousNoteSqid!, cancellationToken);
        Note nextNeighbor = await ResolveNeighborAsync(documentId, nextNoteSqid!, cancellationToken);

        if (prev.NoteId == nextNeighbor.NoteId)
        {
            throw new ArgumentException("Previous and next note cannot be the same.");
        }

        if (prev.SequenceNumber >= nextNeighbor.SequenceNumber)
        {
            throw new ArgumentException("Previous note must be ordered before next note.");
        }

        if (NeedsRebalance(prev.SequenceNumber, nextNeighbor.SequenceNumber))
        {
            await _noteRepository.RebalanceDocumentAsync(documentId, DefaultStep, cancellationToken);
            prev = await ResolveNeighborAsync(documentId, previousNoteSqid!, cancellationToken);
            nextNeighbor = await ResolveNeighborAsync(documentId, nextNoteSqid!, cancellationToken);
        }

        return (prev.SequenceNumber + nextNeighbor.SequenceNumber) / 2m;
    }

    private async Task<Note> ResolveNeighborAsync(long documentId, string noteSqid, CancellationToken cancellationToken)
    {
        if (!_sqidService.TryDecode(noteSqid, out long noteId))
        {
            throw new ArgumentException("Neighbor note sqid is invalid.");
        }

        Note? note = await _noteRepository.GetByIdAsync(noteId, cancellationToken);
        if (note is null || note.DocumentId != documentId)
        {
            throw new KeyNotFoundException("Neighbor note not found in the target document.");
        }

        return note;
    }

    private static bool NeedsRebalance(decimal previous, decimal next)
    {
        return (next - previous) <= MinGapThreshold;
    }

    private bool TryDecodeDocumentAndNote(string documentSqid, string noteSqid, out long documentId, out long noteId)
    {
        bool hasDocument = _sqidService.TryDecode(documentSqid, out documentId);
        bool hasNote = _sqidService.TryDecode(noteSqid, out noteId);
        return hasDocument && hasNote;
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
}
