using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentCoursePerformanceSummaryRepository
{
    Task<StudentCoursePerformanceSummary?> GetTrackedByStudentCourseIdAsync(long studentCourseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentCoursePerformanceSummary>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task AddAsync(StudentCoursePerformanceSummary summary, CancellationToken cancellationToken = default);

    Task UpdateAsync(StudentCoursePerformanceSummary summary, CancellationToken cancellationToken = default);
}
