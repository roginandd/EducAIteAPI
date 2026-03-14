using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class StudentFlashcardRepository : IStudentFlashcardRepository
{
    private readonly ApplicationDbContext _context;

    public StudentFlashcardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFlashcard?> GetByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .AsNoTracking()
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Document.Folder.IsDeleted && !studentFlashcard.Flashcard.Document.FileMetadata.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StudentFlashcard?> GetTrackedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Document.Folder.IsDeleted && !studentFlashcard.Flashcard.Document.FileMetadata.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StudentFlashcard?> GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .IgnoreQueryFilters()
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetReviewQueueByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .AsNoTracking()
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Document.Folder.IsDeleted && !studentFlashcard.Flashcard.Document.FileMetadata.IsDeleted)
            .OrderBy(studentFlashcard => studentFlashcard.NextReviewAt)
            .ThenByDescending(studentFlashcard => studentFlashcard.LapseCount)
            .ThenBy(studentFlashcard => studentFlashcard.FlashcardId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetDueBatchByStudentIdAsync(
        long studentId,
        DateTime now,
        IReadOnlyCollection<long> excludeFlashcardIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StudentFlashcard> query = _context.StudentFlashcards
            .AsNoTracking()
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.NextReviewAt <= now)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Document.Folder.IsDeleted && !studentFlashcard.Flashcard.Document.FileMetadata.IsDeleted);

        if (excludeFlashcardIds.Count > 0)
        {
            query = query.Where(studentFlashcard => !excludeFlashcardIds.Contains(studentFlashcard.FlashcardId));
        }

        return await query
            .OrderBy(studentFlashcard => studentFlashcard.NextReviewAt)
            .ThenByDescending(studentFlashcard => studentFlashcard.LapseCount)
            .ThenBy(studentFlashcard => studentFlashcard.FlashcardId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetTrackedByFlashcardIdAsync(long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentFlashcard> AddAsync(StudentFlashcard studentFlashcard, CancellationToken cancellationToken = default)
    {
        await _context.StudentFlashcards.AddAsync(studentFlashcard, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return studentFlashcard;
    }

    public async Task UpdateAsync(StudentFlashcard studentFlashcard, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
