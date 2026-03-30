namespace EducAIte.Application.DTOs.Response;

public record StudentFlashcardAnalyticsResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string StudentCourseSqid { get; init; } = string.Empty;

    public DateTime? LastAnsweredAt { get; init; }

    public DateTime NextReviewAt { get; init; }

    public decimal EaseFactor { get; init; }

    public int RepetitionCount { get; init; }

    public int IntervalDays { get; init; }

    public int LapseCount { get; init; }

    public string MasteryLevel { get; init; } = string.Empty;

    public decimal ConfidenceScore { get; init; }

    public decimal ConsistencyScore { get; init; }

    public decimal RetentionScore { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public string AiStatus { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;

    public string ImprovementSuggestion { get; init; } = string.Empty;

    public DateTime? AiLastEvaluatedAt { get; init; }

    public DateTime LastComputedAt { get; init; }
}
