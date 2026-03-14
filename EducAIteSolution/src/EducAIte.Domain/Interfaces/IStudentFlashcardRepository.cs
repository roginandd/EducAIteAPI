using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentFlashcardRepository
{
    Task<StudentFlashcard?> GetByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default);

    Task<StudentFlashcard?> GetTrackedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default);

    Task<StudentFlashcard?> GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(long studentId, long flashcardId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentFlashcard>> GetReviewQueueByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentFlashcard>> GetDueBatchByStudentIdAsync(long studentId, DateTime now, IReadOnlyCollection<long> excludeFlashcardIds, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentFlashcard>> GetTrackedByFlashcardIdAsync(long flashcardId, CancellationToken cancellationToken = default);

    Task<StudentFlashcard> AddAsync(StudentFlashcard studentFlashcard, CancellationToken cancellationToken = default);

    Task UpdateAsync(StudentFlashcard studentFlashcard, CancellationToken cancellationToken = default);
}
