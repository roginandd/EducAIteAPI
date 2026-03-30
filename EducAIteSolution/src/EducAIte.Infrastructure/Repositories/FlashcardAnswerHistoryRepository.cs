using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class FlashcardAnswerHistoryRepository : IFlashcardAnswerHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardAnswerHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FlashcardAnswerHistory> AddAsync(FlashcardAnswerHistory history, CancellationToken cancellationToken = default)
    {
        await _context.FlashcardAnswerHistories.AddAsync(history, cancellationToken);
        return history;
    }

    public async Task<IReadOnlyList<FlashcardAnswerHistory>> GetRecentByStudentFlashcardIdAsync(long studentFlashcardId, int take, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardAnswerHistories
            .AsNoTracking()
            .Include(history => history.Evaluation)
            .Where(history => history.StudentFlashcardId == studentFlashcardId)
            .OrderByDescending(history => history.AnsweredAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
