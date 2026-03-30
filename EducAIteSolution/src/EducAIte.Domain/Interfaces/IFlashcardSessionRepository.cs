using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Interfaces;

public interface IFlashcardSessionRepository
{
    Task<IReadOnlyList<FlashcardSession>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<FlashcardSession?> GetActiveByStudentAndScopeAsync(
        long studentId,
        long? studentCourseId,
        FlashcardSessionScopeType scopeType,
        CancellationToken cancellationToken = default);

    Task<FlashcardSession?> GetByIdAndStudentIdAsync(long sessionId, long studentId, CancellationToken cancellationToken = default);

    Task<FlashcardSession?> GetTrackedByIdAndStudentIdAsync(long sessionId, long studentId, CancellationToken cancellationToken = default);

    Task<FlashcardSession> AddAsync(FlashcardSession session, CancellationToken cancellationToken = default);

    Task UpdateAsync(FlashcardSession session, CancellationToken cancellationToken = default);
}
