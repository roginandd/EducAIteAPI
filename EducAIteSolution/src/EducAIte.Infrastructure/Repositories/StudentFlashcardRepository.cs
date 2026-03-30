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
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StudentFlashcard?> GetTrackedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StudentFlashcard?> GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFlashcards
            .IgnoreQueryFilters()
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.AcceptedAnswerAliases)
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .ThenInclude(document => document.Folder)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.FlashcardId == flashcardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StudentFlashcard?> GetTrackedByIdAndStudentIdAsync(long studentFlashcardId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .Where(studentFlashcard => studentFlashcard.StudentFlashcardId == studentFlashcardId)
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetReviewQueueByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
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
        IQueryable<StudentFlashcard> query = CreateBaseQuery()
            .AsNoTracking()
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.NextReviewAt <= now);

        if (excludeFlashcardIds.Count > 0)
        {
            query = query.Where(studentFlashcard => !excludeFlashcardIds.Contains(studentFlashcard.FlashcardId));
        }

        return await query
            .OrderByDescending(studentFlashcard => studentFlashcard.ConsecutiveWrongCount)
            .ThenBy(studentFlashcard => studentFlashcard.NextReviewAt)
            .ThenBy(studentFlashcard => studentFlashcard.FlashcardId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetDueBatchByStudentCourseIdAsync(
        long studentId,
        long studentCourseId,
        DateTime now,
        IReadOnlyCollection<long> excludeFlashcardIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StudentFlashcard> query = CreateBaseQuery()
            .AsNoTracking()
            .Where(studentFlashcard => studentFlashcard.StudentId == studentId)
            .Where(studentFlashcard => studentFlashcard.Flashcard.Note.Document.Folder.StudentCourseId == studentCourseId)
            .Where(studentFlashcard => studentFlashcard.NextReviewAt <= now);

        if (excludeFlashcardIds.Count > 0)
        {
            query = query.Where(studentFlashcard => !excludeFlashcardIds.Contains(studentFlashcard.FlashcardId));
        }

        return await query
            .OrderByDescending(studentFlashcard => studentFlashcard.ConsecutiveWrongCount)
            .ThenBy(studentFlashcard => studentFlashcard.NextReviewAt)
            .ThenBy(studentFlashcard => studentFlashcard.FlashcardId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentFlashcard>> GetAllByStudentCourseIdAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(studentFlashcard => studentFlashcard.Flashcard.Note.Document.Folder.StudentCourseId == studentCourseId)
            .OrderBy(studentFlashcard => studentFlashcard.FlashcardId)
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
        return studentFlashcard;
    }

    public async Task UpdateAsync(StudentFlashcard studentFlashcard, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    private IQueryable<StudentFlashcard> CreateBaseQuery()
    {
        return _context.StudentFlashcards
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.AcceptedAnswerAliases)
            .Include(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .ThenInclude(document => document.Folder)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Note.Document.Folder.IsDeleted)
            .Where(studentFlashcard => !studentFlashcard.Flashcard.Note.Document.FileMetadata.IsDeleted);
    }
}
