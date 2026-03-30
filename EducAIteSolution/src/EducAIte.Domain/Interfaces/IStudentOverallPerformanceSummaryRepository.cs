using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentOverallPerformanceSummaryRepository
{
    Task<StudentOverallPerformanceSummary?> GetTrackedByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task AddAsync(StudentOverallPerformanceSummary summary, CancellationToken cancellationToken = default);

    Task UpdateAsync(StudentOverallPerformanceSummary summary, CancellationToken cancellationToken = default);
}
