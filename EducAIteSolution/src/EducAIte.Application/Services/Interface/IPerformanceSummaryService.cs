using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IPerformanceSummaryService
{
    Task RefreshStudentCourseSummaryAsync(long studentCourseId, CancellationToken cancellationToken = default);

    Task RefreshOverallSummaryAsync(long studentId, CancellationToken cancellationToken = default);

    Task<StudentAnalyticsDashboardResponse> GetDashboardAsync(long studentId, CancellationToken cancellationToken = default);

    Task<StudentCoursePerformanceSummaryEvaluationContextResponse?> GetStudentCourseSummaryEvaluationContextAsync(
        string studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentOverallPerformanceSummaryEvaluationContextResponse?> GetOverallSummaryEvaluationContextAsync(
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentCoursePerformanceSummaryResponse> ApplyStudentCourseAiSummaryAsync(
        string studentCourseSqid,
        UpsertPerformanceSummaryAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentOverallPerformanceSummaryResponse> ApplyOverallAiSummaryAsync(
        UpsertPerformanceSummaryAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentCoursePerformanceSummaryResponse?> GetStudentCourseSummaryAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentOverallPerformanceSummaryResponse?> GetOverallSummaryAsync(long studentId, CancellationToken cancellationToken = default);
}
