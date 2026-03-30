using System.Net.Http.Json;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;

namespace EducAIte.Application.Services.Implementation;

public sealed class EducaiteAiStudentPerformanceClient : IStudentPerformanceAiClient
{
    private readonly HttpClient _httpClient;

    public EducaiteAiStudentPerformanceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<GeneratedPerformanceSummaryAiResponse> EvaluateCourseSummaryAsync(
        StudentCoursePerformanceSummaryEvaluationContextResponse context,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("internal/student-performance/course-summary/evaluate", context, cancellationToken);
    }

    public Task<GeneratedPerformanceSummaryAiResponse> EvaluateOverallSummaryAsync(
        StudentOverallPerformanceSummaryEvaluationContextResponse context,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("internal/student-performance/overall-summary/evaluate", context, cancellationToken);
    }

    private async Task<GeneratedPerformanceSummaryAiResponse> PostAsync<TRequest>(
        string relativeUrl,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(relativeUrl, request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"educAIteAI performance summary evaluation failed with status {(int)response.StatusCode}: {responseBody}");
        }

        GeneratedPerformanceSummaryAiResponse? parsedResponse =
            await response.Content.ReadFromJsonAsync<GeneratedPerformanceSummaryAiResponse>(cancellationToken: cancellationToken);

        return parsedResponse
            ?? throw new InvalidOperationException("educAIteAI performance summary evaluation returned an empty response body.");
    }
}
