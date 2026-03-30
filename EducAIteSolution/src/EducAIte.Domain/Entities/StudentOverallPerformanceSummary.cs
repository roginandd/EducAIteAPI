namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class StudentOverallPerformanceSummary
{
    public long StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public int TrackedCoursesCount { get; private set; }
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

    private StudentOverallPerformanceSummary()
    {
    }

    public StudentOverallPerformanceSummary(long studentId)
    {
        StudentId = ValidatePositiveId(studentId, nameof(studentId));
        RiskLevel = FlashcardRiskLevel.High;
        AiStatus = AiEvaluationStatus.InsufficientSignal;
        LastComputedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Recompute(IReadOnlyList<StudentCoursePerformanceSummary> courseSummaries, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(courseSummaries);

        TrackedCoursesCount = courseSummaries.Count;
        TrackedFlashcardsCount = courseSummaries.Sum(summary => summary.TrackedFlashcardsCount);
        MasteredFlashcardsCount = courseSummaries.Sum(summary => summary.MasteredFlashcardsCount);

        if (courseSummaries.Count == 0)
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

        FlashcardAccuracyRate = decimal.Round(courseSummaries.Average(summary => summary.FlashcardAccuracyRate), 4);
        LearningRetentionRate = decimal.Round(courseSummaries.Average(summary => summary.LearningRetentionRate), 2);
        ConfidenceScore = decimal.Round(courseSummaries.Average(summary => summary.ConfidenceScore), 2);
        OverallPerformanceScore = decimal.Round(courseSummaries.Average(summary => summary.OverallPerformanceScore), 2);
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
