using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class FlashcardRepository : IFlashcardRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Flashcard?> GetByIdAsync(long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FlashcardId == flashcardId, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .AnyAsync(f => f.FlashcardId == flashcardId, cancellationToken);
    }

    public async Task<Flashcard?> GetByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.FlashcardId == flashcardId)
            .Where(f => f.Document.Folder.StudentId == studentId)
            .Where(f => !f.Document.Folder.IsDeleted && !f.Document.FileMetadata.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Flashcard?> GetTrackedByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .Where(f => f.FlashcardId == flashcardId)
            .Where(f => f.Document.Folder.StudentId == studentId)
            .Where(f => !f.Document.Folder.IsDeleted && !f.Document.FileMetadata.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.Document.Folder.StudentId == studentId)
            .Where(f => !f.Document.Folder.IsDeleted && !f.Document.FileMetadata.IsDeleted)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetAllByDocumentIdAndStudentIdAsync(long documentId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .Where(f => f.DocumentId == documentId)
            .Where(f => f.Document.Folder.StudentId == studentId)
            .Where(f => !f.Document.Folder.IsDeleted && !f.Document.FileMetadata.IsDeleted)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetUntrackedByStudentIdAsync(
        long studentId,
        IReadOnlyCollection<long> excludeFlashcardIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Flashcard> query = _context.Flashcards
            .AsNoTracking()
            .Where(f => f.Document.Folder.StudentId == studentId)
            .Where(f => !f.Document.Folder.IsDeleted && !f.Document.FileMetadata.IsDeleted)
            .Where(f => !_context.StudentFlashcards
                .IgnoreQueryFilters()
                .Any(sf => sf.StudentId == studentId && sf.FlashcardId == f.FlashcardId));

        if (excludeFlashcardIds.Count > 0)
        {
            query = query.Where(f => !excludeFlashcardIds.Contains(f.FlashcardId));
        }

        return await query
            .OrderBy(f => f.FlashcardId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Flashcard> AddAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.Add(flashcard);
        await _context.SaveChangesAsync(cancellationToken);
        return flashcard;
    }

    public async Task UpdateAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
