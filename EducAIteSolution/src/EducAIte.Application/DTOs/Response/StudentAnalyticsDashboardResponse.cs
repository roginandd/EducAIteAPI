namespace EducAIte.Application.DTOs.Response;

public record StudentAnalyticsDashboardResponse
{
    public OverallPerformanceDTO OverallPerformance { get; init; } = new();

    public LearningTrendAnalysisDTO LearningTrendAnalysis { get; init; } = new();

    public BestPerformingCourseDTO BestPerformingCourse { get; init; } = new();

    public LearningRetentionRateDTO LearningRetentionRate { get; init; } = new();

    public PerformanceSummaryRateDTO PerformanceSummaryRate { get; init; } = new();
}

public record OverallPerformanceDTO
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

    public DateTime? LastComputedAt { get; init; }
}

public record LearningTrendAnalysisDTO
{
    public IReadOnlyList<CourseStudyTimeItemDTO> Items { get; init; } = [];
}

public record BestPerformingCourseDTO
{
    public IReadOnlyList<CourseOverallPerformanceItemDTO> Items { get; init; } = [];
}

public record LearningRetentionRateDTO
{
    public IReadOnlyList<CourseLearningRetentionItemDTO> Items { get; init; } = [];
}

public record PerformanceSummaryRateDTO
{
    public IReadOnlyList<CoursePerformanceSummaryItemDTO> Items { get; init; } = [];
}

public record CourseStudyTimeItemDTO
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string CourseName { get; init; } = string.Empty;

    public required string EdpCode { get; init; } = string.Empty;

    public decimal StudyTimeHours { get; init; }
}

public record CourseOverallPerformanceItemDTO
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string CourseName { get; init; } = string.Empty;

    public required string EdpCode { get; init; } = string.Empty;

    public decimal OverallPerformanceScore { get; init; }
}

public record CourseLearningRetentionItemDTO
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string CourseName { get; init; } = string.Empty;

    public required string EdpCode { get; init; } = string.Empty;

    public decimal LearningRetentionRate { get; init; }
}

public record CoursePerformanceSummaryItemDTO
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string CourseName { get; init; } = string.Empty;

    public required string EdpCode { get; init; } = string.Empty;

    public decimal OverallPerformanceScore { get; init; }
}
