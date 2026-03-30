using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class FlashcardSessionRepository : IFlashcardSessionRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FlashcardSession>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessions
            .AsNoTracking()
            .Where(session => session.StudentId == studentId)
            .Where(session => !session.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<FlashcardSession?> GetActiveByStudentAndScopeAsync(
        long studentId,
        long? studentCourseId,
        FlashcardSessionScopeType scopeType,
        CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessions
            .AsNoTracking()
            .Where(session => session.StudentId == studentId)
            .Where(session => session.StudentCourseId == studentCourseId)
            .Where(session => session.ScopeType == scopeType)
            .Where(session => session.Status == FlashcardSessionStatus.InProgress)
            .OrderByDescending(session => session.LastActiveAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FlashcardSession?> GetByIdAndStudentIdAsync(long sessionId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.FlashcardSessionId == sessionId && session.StudentId == studentId, cancellationToken);
    }

    public async Task<FlashcardSession?> GetTrackedByIdAndStudentIdAsync(long sessionId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessions
            .FirstOrDefaultAsync(session => session.FlashcardSessionId == sessionId && session.StudentId == studentId, cancellationToken);
    }

    public async Task<FlashcardSession> AddAsync(FlashcardSession session, CancellationToken cancellationToken = default)
    {
        await _context.FlashcardSessions.AddAsync(session, cancellationToken);
        return session;
    }

    public async Task UpdateAsync(FlashcardSession session, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
