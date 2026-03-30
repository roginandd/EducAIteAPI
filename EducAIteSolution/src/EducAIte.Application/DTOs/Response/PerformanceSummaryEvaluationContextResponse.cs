namespace EducAIte.Application.DTOs.Response;

public record StudentCoursePerformanceSummaryEvaluationContextResponse
{
    public string StudentCourseSqid { get; init; } = string.Empty;

    public string CourseName { get; init; } = string.Empty;

    public string EdpCode { get; init; } = string.Empty;

    public CoursePerformanceSummarySnapshotResponse Summary { get; init; } = new();

    public IReadOnlyList<CoursePerformanceSummaryFlashcardContextResponse> TopRiskFlashcards { get; init; } = [];
}

public record StudentOverallPerformanceSummaryEvaluationContextResponse
{
    public OverallPerformanceSummarySnapshotResponse Summary { get; init; } = new();

    public IReadOnlyList<CoursePerformanceSummaryBreakdownResponse> CourseBreakdown { get; init; } = [];
}

public record CoursePerformanceSummarySnapshotResponse
{
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

public record OverallPerformanceSummarySnapshotResponse
{
    public int TrackedCoursesCount { get; init; }

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

public record CoursePerformanceSummaryFlashcardContextResponse
{
    public string FlashcardSqid { get; init; } = string.Empty;

    public string Question { get; init; } = string.Empty;

    public string MasteryLevel { get; init; } = string.Empty;

    public decimal ConfidenceScore { get; init; }

    public decimal RetentionScore { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;
}

public record CoursePerformanceSummaryBreakdownResponse
{
    public string StudentCourseSqid { get; init; } = string.Empty;

    public string CourseName { get; init; } = string.Empty;

    public string EdpCode { get; init; } = string.Empty;

    public int TrackedFlashcardsCount { get; init; }

    public int MasteredFlashcardsCount { get; init; }

    public decimal FlashcardAccuracyRate { get; init; }

    public decimal LearningRetentionRate { get; init; }

    public decimal ConfidenceScore { get; init; }

    public decimal OverallPerformanceScore { get; init; }

    public string RiskLevel { get; init; } = string.Empty;

    public string AiInsight { get; init; } = string.Empty;
}
