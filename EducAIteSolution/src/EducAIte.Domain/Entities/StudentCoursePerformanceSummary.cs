namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class StudentCoursePerformanceSummary
{
    public long StudentCourseId { get; private set; }
    public StudentCourse StudentCourse { get; private set; } = null!;
    public int TrackedFlashcardsCount { get; private set; }
    public int MasteredFlashcardsCount { get; private set; }
    public decimal FlashcardAccuracyRate { get; private set; }
    public decimal LearningRetentionRate { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public decimal OverallPerformanceScore { get; private set; }
    public FlashcardRiskLevel RiskLevel { get; private set; }
    public string AiInsight { get; private set; } = string.Empty;
    public string ImprovementSuggestion { get; private set; } = string.Empty;
    public AiEvaluationStatus AiStatus { get; private set; }
    public DateTime? AiLastEvaluatedAt { get; private set; }
    public DateTime LastComputedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StudentCoursePerformanceSummary()
    {
    }

    public StudentCoursePerformanceSummary(long studentCourseId)
    {
        StudentCourseId = ValidatePositiveId(studentCourseId, nameof(studentCourseId));
        RiskLevel = FlashcardRiskLevel.High;
        AiStatus = AiEvaluationStatus.InsufficientSignal;
        LastComputedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Recompute(IReadOnlyList<StudentFlashcard> progressRows, IReadOnlyList<StudentFlashcardAnalytics> analyticsRows, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(progressRows);
        ArgumentNullException.ThrowIfNull(analyticsRows);

        TrackedFlashcardsCount = progressRows.Count;
        MasteredFlashcardsCount = analyticsRows.Count(analytics => analytics.MasteryLevel == FlashcardMasteryLevel.Mastered);

        if (progressRows.Count == 0)
        {
            FlashcardAccuracyRate = 0m;
            LearningRetentionRate = 0m;
            ConfidenceScore = 0m;
            OverallPerformanceScore = 0m;
            RiskLevel = FlashcardRiskLevel.High;
            LastComputedAt = now.ToUniversalTime();
            UpdatedAt = LastComputedAt;
            return;
        }

        decimal totalAttempts = progressRows.Sum(progress => progress.TotalAttempts);
        decimal totalCorrect = progressRows.Sum(progress => progress.CorrectCount);
        FlashcardAccuracyRate = totalAttempts == 0m ? 0m : decimal.Round(totalCorrect / totalAttempts, 4);
        LearningRetentionRate = decimal.Round(analyticsRows.DefaultIfEmpty().Average(analytics => analytics?.RetentionScore ?? 0m), 2);
        ConfidenceScore = decimal.Round(analyticsRows.DefaultIfEmpty().Average(analytics => analytics?.ConfidenceScore ?? 0m), 2);
        OverallPerformanceScore = decimal.Round((FlashcardAccuracyRate * 100m * 0.40m) + (LearningRetentionRate * 0.35m) + (ConfidenceScore * 0.25m), 2);
        RiskLevel = OverallPerformanceScore >= 80m ? FlashcardRiskLevel.Low : OverallPerformanceScore >= 60m ? FlashcardRiskLevel.Medium : FlashcardRiskLevel.High;
        LastComputedAt = now.ToUniversalTime();
        UpdatedAt = LastComputedAt;
    }

    public void ApplyAiSummary(string insight, string suggestion, AiEvaluationStatus status, DateTime now)
    {
        AiInsight = (insight ?? string.Empty).Trim();
        ImprovementSuggestion = (suggestion ?? string.Empty).Trim();
        AiStatus = status;
        AiLastEvaluatedAt = now.ToUniversalTime();
        UpdatedAt = AiLastEvaluatedAt.Value;
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }
}
