namespace EducAIte.Application.DTOs.Request;

public record UpsertFlashcardAnalyticsAiRequest
{
    public DateTime NextReviewAt { get; init; }

    public decimal EaseFactor { get; init; }

    public int RepetitionCount { get; init; }

    public int IntervalDays { get; init; }

    public int LapseCount { get; init; }

    public required string MasteryLevel { get; init; } = string.Empty;

    public decimal ConfidenceScore { get; init; }

    public decimal ConsistencyScore { get; init; }

    public decimal RetentionScore { get; init; }

    public required string RiskLevel { get; init; } = string.Empty;

    public required string AiStatus { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;

    public string ImprovementSuggestion { get; init; } = string.Empty;
}
