using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;


public sealed class NoteRepository : INoteRepository
{
    private readonly ApplicationDbContext _context;

    public NoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsOwnedByStudentAsync(long noteId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AsNoTracking()
            .Where(note => note.NoteId == noteId)
            .Where(note => !note.Document.Folder.IsDeleted)
            .AnyAsync(note => note.Document.Folder.StudentId == studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Note>> GetAllByDocumentIdAndStudentIdAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .ThenInclude(d => d.Folder)
            .Where(n => n.DocumentId == documentId)
            .Where(n => n.Document.Folder.StudentId == studentId)
            .OrderBy(n => n.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Note>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Note> notes = await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .ThenInclude(d => d.Folder)
            .Where(n => n.Document.Folder.StudentId == studentId)
            .OrderBy(n => n.SequenceNumber)
            .ToListAsync(cancellationToken);

        return notes;
    }

    public async Task<IReadOnlyList<Note>> GetAllByDocumentIdsAndStudentIdAsync(
        IReadOnlyCollection<long> documentIds,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        if (documentIds.Count == 0)
        {
            return [];
        }

        return await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .ThenInclude(d => d.Folder)
            .Where(n => documentIds.Contains(n.DocumentId))
            .Where(n => n.Document.Folder.StudentId == studentId)
            .OrderBy(n => n.DocumentId)
            .ThenBy(n => n.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Note>> SearchByFolderIdsAndStudentIdAsync(
        IReadOnlyCollection<long> folderIds,
        long studentId,
        string query,
        CancellationToken cancellationToken = default)
    {
        if (folderIds.Count == 0)
        {
            return [];
        }

        string pattern = $"%{query}%";

        return await _context
            .Notes
            .AsNoTracking()
            .Include(note => note.Document)
            .ThenInclude(document => document.Folder)
            .Where(note =>
                folderIds.Contains(note.Document.FolderId) &&
                note.Document.Folder.StudentId == studentId &&
                !note.Document.Folder.IsDeleted &&
                !note.Document.IsDeleted &&
                !note.IsDeleted)
            .Where(note =>
                EF.Functions.ILike(note.Name, pattern) ||
                EF.Functions.ILike(note.NoteContent, pattern))
            .OrderBy(note => note.Name)
            .ThenBy(note => note.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Note?> GetLastByDocumentIdAsync(long documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AsNoTracking()
            .Where(n => n.DocumentId == documentId)
            .OrderByDescending(n => n.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Add(note);
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        bool isTracked = _context.ChangeTracker.Entries<Note>().Any(entry => entry.Entity.NoteId == note.NoteId);
        if (!isTracked)
        {
            _context.Attach(note);
            _context.Entry(note).State = EntityState.Modified;
        }

    }

    public async Task<Note?> GetByIdAsync(long noteId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .FirstOrDefaultAsync(n => n.NoteId == noteId, cancellationToken);

        return note;
    }

    public async Task<Note?> GetTrackedByIdAsync(long noteId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .Include(n => n.Document)
            .FirstOrDefaultAsync(n => n.NoteId == noteId, cancellationToken);

        return note;
    }

    public async Task<Note?> GetTrackedByIdAndDocumentIdAsync(long noteId, long documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.Document)
            .FirstOrDefaultAsync(n => n.NoteId == noteId && n.DocumentId == documentId, cancellationToken);
    }

    public async Task DeleteAsync(long noteId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .FirstOrDefaultAsync(n => n.NoteId == noteId, cancellationToken);

        if (note is null)
        {
            return;
        }

        note.MarkDeleted();
    }

    public async Task RebalanceDocumentAsync(long documentId, decimal step, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        List<Note> notes = await _context.Notes
            .Where(n => n.DocumentId == documentId)
            .OrderBy(n => n.SequenceNumber)
            .ToListAsync(cancellationToken);

        decimal current = step;
        foreach (Note note in notes)
        {
            note.UpdateSequenceNumber(current);
            current += step;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

}
