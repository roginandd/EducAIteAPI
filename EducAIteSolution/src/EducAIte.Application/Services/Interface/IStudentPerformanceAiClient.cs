using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IStudentPerformanceAiClient
{
    Task<GeneratedPerformanceSummaryAiResponse> EvaluateCourseSummaryAsync(
        StudentCoursePerformanceSummaryEvaluationContextResponse context,
        CancellationToken cancellationToken = default);

    Task<GeneratedPerformanceSummaryAiResponse> EvaluateOverallSummaryAsync(
        StudentOverallPerformanceSummaryEvaluationContextResponse context,
        CancellationToken cancellationToken = default);
}
