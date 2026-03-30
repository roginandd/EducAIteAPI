namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;
using EducAIte.Domain.ValueObjects;

public class StudentFlashcardAnalytics
{
    public long StudentFlashcardId { get; private set; }
    public StudentFlashcard StudentFlashcard { get; private set; } = null!;

    public DateTime? LastAnsweredAt { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public decimal EaseFactor { get; private set; }
    public int RepetitionCount { get; private set; }
    public int IntervalDays { get; private set; }
    public int LapseCount { get; private set; }
    public FlashcardMasteryLevel MasteryLevel { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public decimal ConsistencyScore { get; private set; }
    public decimal RetentionScore { get; private set; }
    public FlashcardRiskLevel RiskLevel { get; private set; }
    public string AiInsight { get; private set; } = string.Empty;
    public string ImprovementSuggestion { get; private set; } = string.Empty;
    public AiEvaluationStatus AiStatus { get; private set; }
    public DateTime? AiLastEvaluatedAt { get; private set; }
    public DateTime LastComputedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StudentFlashcardAnalytics()
    {
    }

    public StudentFlashcardAnalytics(long studentFlashcardId)
    {
        StudentFlashcardId = ValidatePositiveId(studentFlashcardId, nameof(studentFlashcardId));
        NextReviewAt = DateTime.UtcNow;
        EaseFactor = 0m;
        RepetitionCount = 0;
        IntervalDays = 0;
        LapseCount = 0;
        ConfidenceScore = 0m;
        ConsistencyScore = 0m;
        RetentionScore = 0m;
        MasteryLevel = FlashcardMasteryLevel.New;
        RiskLevel = FlashcardRiskLevel.High;
        AiStatus = AiEvaluationStatus.InsufficientSignal;
        LastComputedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void InitializeDefaults(StudentFlashcard progress, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(progress);

        DateTime initializedAt = now.ToUniversalTime();
        LastAnsweredAt = progress.LastReviewedAt;
        NextReviewAt = progress.NextReviewAt;
        EaseFactor = 0m;
        RepetitionCount = 0;
        IntervalDays = 0;
        LapseCount = 0;
        MasteryLevel = FlashcardMasteryLevel.New;
        ConfidenceScore = 0m;
        ConsistencyScore = 0m;
        RetentionScore = 0m;
        RiskLevel = FlashcardRiskLevel.High;
        AiInsight = string.Empty;
        ImprovementSuggestion = string.Empty;
        AiStatus = AiEvaluationStatus.InsufficientSignal;
        AiLastEvaluatedAt = null;
        LastComputedAt = initializedAt;
        UpdatedAt = initializedAt;
    }

    public void RecomputeFrom(StudentFlashcard progress, IReadOnlyList<FlashcardAnswerHistory> recentAnswers, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(recentAnswers);

        LastAnsweredAt = progress.LastReviewedAt;
        NextReviewAt = progress.NextReviewAt;
        RepetitionCount = progress.ConsecutiveCorrectCount;
        LapseCount = progress.LapseCount;
        IntervalDays = progress.LastReviewedAt.HasValue
            ? Math.Max(0, (int)Math.Ceiling((progress.NextReviewAt - progress.LastReviewedAt.Value).TotalDays))
            : 0;

        decimal attemptCount = Math.Max(1, progress.TotalAttempts);
        decimal accuracyRate = progress.CorrectCount / attemptCount;
        decimal lapsePenalty = progress.LapseCount / attemptCount;
        decimal averageQuality = recentAnswers.Count == 0
            ? (accuracyRate * 5m)
            : decimal.Parse(recentAnswers.Average(answer => answer.FinalQualityScore).ToString("F2"));

        EaseFactor = decimal.Round(Math.Clamp(1.3m + (averageQuality / 5m) * 1.7m, 1.3m, 3.0m), 2);
        ConfidenceScore = decimal.Round(Math.Clamp((accuracyRate * 70m) + ((averageQuality / 5m) * 30m), 0m, 100m), 2);
        ConsistencyScore = decimal.Round(CalculateConsistencyScore(recentAnswers, accuracyRate), 2);
        RetentionScore = decimal.Round(Math.Clamp((accuracyRate * 100m) - (lapsePenalty * 30m) + (ConsistencyScore * 0.2m), 0m, 100m), 2);
        MasteryLevel = DeriveMasteryLevel(progress, accuracyRate, averageQuality);
        RiskLevel = DeriveRiskLevel(ConfidenceScore, RetentionScore, progress.ConsecutiveWrongCount);
        LastComputedAt = now.ToUniversalTime();
        UpdatedAt = LastComputedAt;
    }

    public void MarkAiPending(DateTime now)
    {
        AiStatus = AiEvaluationStatus.Pending;
        UpdatedAt = now.ToUniversalTime();
    }

    public void ApplyAiEvaluation(FlashcardAiEvaluationResult result, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(result);

        ConfidenceScore = decimal.Round(Math.Clamp(result.ConfidenceScore, 0m, 100m), 2);
        RiskLevel = result.RiskLevel;
        AiInsight = (result.Insight ?? string.Empty).Trim();
        ImprovementSuggestion = (result.ImprovementSuggestion ?? string.Empty).Trim();
        AiStatus = result.Status;
        AiLastEvaluatedAt = now.ToUniversalTime();
        UpdatedAt = AiLastEvaluatedAt.Value;
    }

    public void ApplyExternalEvaluation(FlashcardAnalyticsAiEvaluationResult result, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(result);

        DateTime evaluationTime = now.ToUniversalTime();

        NextReviewAt = result.NextReviewAt.ToUniversalTime();
        EaseFactor = decimal.Round(Math.Clamp(result.EaseFactor, 1.3m, 3.0m), 2);
        RepetitionCount = Math.Max(0, result.RepetitionCount);
        IntervalDays = Math.Max(0, result.IntervalDays);
        LapseCount = Math.Max(0, result.LapseCount);
        MasteryLevel = result.MasteryLevel;
        ConfidenceScore = decimal.Round(Math.Clamp(result.ConfidenceScore, 0m, 100m), 2);
        ConsistencyScore = decimal.Round(Math.Clamp(result.ConsistencyScore, 0m, 100m), 2);
        RetentionScore = decimal.Round(Math.Clamp(result.RetentionScore, 0m, 100m), 2);
        RiskLevel = result.RiskLevel;
        AiInsight = (result.AiInsight ?? string.Empty).Trim();
        ImprovementSuggestion = (result.ImprovementSuggestion ?? string.Empty).Trim();
        AiStatus = result.AiStatus;
        AiLastEvaluatedAt = evaluationTime;
        LastComputedAt = evaluationTime;
        UpdatedAt = evaluationTime;
    }

    private static decimal CalculateConsistencyScore(IReadOnlyList<FlashcardAnswerHistory> recentAnswers, decimal fallbackAccuracyRate)
    {
        if (recentAnswers.Count <= 1)
        {
            return fallbackAccuracyRate * 100m;
        }

        decimal transitions = 0m;
        for (int index = 1; index < recentAnswers.Count; index++)
        {
            if (recentAnswers[index].WasAcceptedAsCorrect != recentAnswers[index - 1].WasAcceptedAsCorrect)
            {
                transitions += 1m;
            }
        }

        decimal penalty = (transitions / (recentAnswers.Count - 1)) * 60m;
        return Math.Clamp(100m - penalty, 0m, 100m);
    }

    private static FlashcardMasteryLevel DeriveMasteryLevel(StudentFlashcard progress, decimal accuracyRate, decimal averageQuality)
    {
        if (progress.ReviewCount == 0)
        {
            return FlashcardMasteryLevel.New;
        }

        if (progress.ConsecutiveCorrectCount >= 4 && accuracyRate >= 0.85m && averageQuality >= 4.5m)
        {
            return FlashcardMasteryLevel.Mastered;
        }

        if (progress.ConsecutiveCorrectCount >= 2 && accuracyRate >= 0.70m)
        {
            return FlashcardMasteryLevel.Review;
        }

        return FlashcardMasteryLevel.Learning;
    }

    private static FlashcardRiskLevel DeriveRiskLevel(decimal confidenceScore, decimal retentionScore, int consecutiveWrongCount)
    {
        if (consecutiveWrongCount >= 2 || confidenceScore < 45m || retentionScore < 45m)
        {
            return FlashcardRiskLevel.High;
        }

        if (confidenceScore < 70m || retentionScore < 70m)
        {
            return FlashcardRiskLevel.Medium;
        }

        return FlashcardRiskLevel.Low;
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
