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

    public async Task<IReadOnlyList<Note>> GetAllByDocumentIdAsync(long documentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Note> notes = await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .Where(n => n.DocumentId == documentId)
            .OrderBy(n => n.SequenceNumber) 
            .ToListAsync(cancellationToken);

            return notes;
    }

    public async Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByExternalIdAsync(note.ExternalId, cancellationToken) ?? note;
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        bool isTracked = _context.ChangeTracker.Entries<Note>().Any(entry => entry.Entity.NoteId == note.NoteId);
        if (!isTracked)
        {
            _context.Attach(note);
            _context.Entry(note).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Note?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .AsNoTracking()
            .Include(n => n.Document)
            .FirstOrDefaultAsync(n => n.ExternalId == externalId, cancellationToken);

        return note;
    }

    public async Task<Note?> GetTrackedByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .Include(n => n.Document)
            .FirstOrDefaultAsync(n => n.ExternalId == externalId, cancellationToken);

        return note;
    }

    public async Task<bool> SoftDeleteByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default)
    {
        Note? note = await _context
            .Notes
            .FirstOrDefaultAsync(n => n.ExternalId == externalId, cancellationToken);

        if (note is null)
        {
            return false;
        }

        note.MarkDeleted();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

}
