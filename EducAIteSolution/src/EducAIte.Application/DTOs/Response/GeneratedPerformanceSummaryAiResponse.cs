namespace EducAIte.Application.DTOs.Response;

public sealed record GeneratedPerformanceSummaryAiResponse
{
    public string AiStatus { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;

    public string ImprovementSuggestion { get; init; } = string.Empty;
}
