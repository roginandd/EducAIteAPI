using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentFlashcardAnalyticsRepository
{
    Task<StudentFlashcardAnalytics?> GetByStudentFlashcardIdAsync(long studentFlashcardId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardAnalytics?> GetTrackedByStudentFlashcardIdAsync(long studentFlashcardId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentFlashcardAnalytics>> GetPrioritizedCandidatesAsync(
        long studentId,
        long? studentCourseId,
        DateTime now,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentFlashcardAnalytics>> GetByStudentCourseIdAsync(long studentCourseId, CancellationToken cancellationToken = default);

    Task AddAsync(StudentFlashcardAnalytics analytics, CancellationToken cancellationToken = default);

    Task UpdateAsync(StudentFlashcardAnalytics analytics, CancellationToken cancellationToken = default);
}
