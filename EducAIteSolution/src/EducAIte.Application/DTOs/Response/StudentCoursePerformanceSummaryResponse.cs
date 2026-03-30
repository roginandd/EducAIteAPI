namespace EducAIte.Application.DTOs.Response;

public record StudentCoursePerformanceSummaryResponse
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public int TrackedFlashcardsCount { get; init; }

    public int MasteredFlashcardsCount { get; init; }

    public decimal FlashcardAccuracyRate { get; init; }

    public decimal LearningRetentionRate { get; init; }

    public decimal ConfidenceScore { get; init; }

    public decimal OverallPerformanceScore { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public string AiStatus { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;

    public string ImprovementSuggestion { get; init; } = string.Empty;

    public DateTime LastComputedAt { get; init; }
}
