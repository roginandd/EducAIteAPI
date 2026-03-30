using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IFlashcardSessionItemRepository
{
    Task<IReadOnlyList<FlashcardSessionItem>> GetBySessionIdAsync(long sessionId, CancellationToken cancellationToken = default);

    Task<FlashcardSessionItem?> GetTrackedByIdAsync(long sessionItemId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<FlashcardSessionItem> items, CancellationToken cancellationToken = default);

    Task UpdateAsync(FlashcardSessionItem item, CancellationToken cancellationToken = default);
}
