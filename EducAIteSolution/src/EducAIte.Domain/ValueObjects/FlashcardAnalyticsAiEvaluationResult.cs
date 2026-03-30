namespace EducAIte.Domain.ValueObjects;

using EducAIte.Domain.Enum;

public sealed record FlashcardAnalyticsAiEvaluationResult(
    DateTime NextReviewAt,
    decimal EaseFactor,
    int RepetitionCount,
    int IntervalDays,
    int LapseCount,
    FlashcardMasteryLevel MasteryLevel,
    decimal ConfidenceScore,
    decimal ConsistencyScore,
    decimal RetentionScore,
    FlashcardRiskLevel RiskLevel,
    string AiInsight,
    string ImprovementSuggestion,
    AiEvaluationStatus AiStatus);
