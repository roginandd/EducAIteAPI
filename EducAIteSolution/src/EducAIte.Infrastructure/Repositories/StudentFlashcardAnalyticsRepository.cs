using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class StudentFlashcardAnalyticsRepository : IStudentFlashcardAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public StudentFlashcardAnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFlashcardAnalytics?> GetByStudentFlashcardIdAsync(long studentFlashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcardAnalytics
            .AsNoTracking()
            .FirstOrDefaultAsync(analytics => analytics.StudentFlashcardId == studentFlashcardId, cancellationToken);
    }

    public async Task<StudentFlashcardAnalytics?> GetTrackedByStudentFlashcardIdAsync(long studentFlashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcardAnalytics
            .FirstOrDefaultAsync(analytics => analytics.StudentFlashcardId == studentFlashcardId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcardAnalytics>> GetPrioritizedCandidatesAsync(
        long studentId,
        long? studentCourseId,
        DateTime now,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StudentFlashcardAnalytics> query = _context.StudentFlashcardAnalytics
            .AsNoTracking()
            .Include(analytics => analytics.StudentFlashcard)
            .ThenInclude(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .Where(analytics => analytics.StudentFlashcard.StudentId == studentId)
            .Where(analytics => !analytics.StudentFlashcard.Flashcard.Note.Document.Folder.IsDeleted);

        if (studentCourseId.HasValue)
        {
            query = query.Where(analytics => analytics.StudentFlashcard.Flashcard.Note.Document.Folder.StudentCourseId == studentCourseId.Value);
        }

        return await query
            .OrderBy(analytics => analytics.NextReviewAt > now)
            .ThenByDescending(analytics => analytics.RiskLevel)
            .ThenBy(analytics => analytics.NextReviewAt)
            .ThenBy(analytics => analytics.ConfidenceScore)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcardAnalytics>> GetByStudentCourseIdAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcardAnalytics
            .AsNoTracking()
            .Include(analytics => analytics.StudentFlashcard)
            .ThenInclude(studentFlashcard => studentFlashcard.Flashcard)
            .Where(analytics => analytics.StudentFlashcard.Flashcard.Note.Document.Folder.StudentCourseId == studentCourseId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StudentFlashcardAnalytics analytics, CancellationToken cancellationToken = default)
    {
        await _context.StudentFlashcardAnalytics.AddAsync(analytics, cancellationToken);
    }

    public async Task UpdateAsync(StudentFlashcardAnalytics analytics, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
