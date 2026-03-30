using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IFlashcardAnswerHistoryRepository
{
    Task<FlashcardAnswerHistory> AddAsync(FlashcardAnswerHistory history, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardAnswerHistory>> GetRecentByStudentFlashcardIdAsync(long studentFlashcardId, int take, CancellationToken cancellationToken = default);
}
