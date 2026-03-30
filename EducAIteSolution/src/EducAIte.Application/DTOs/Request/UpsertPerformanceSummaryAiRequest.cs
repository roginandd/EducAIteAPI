namespace EducAIte.Application.DTOs.Request;

public record UpsertPerformanceSummaryAiRequest
{
    public DateTime BasisLastComputedAt { get; init; }

    public string AiStatus { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;

    public string ImprovementSuggestion { get; init; } = string.Empty;
}
