namespace EducAIte.Domain.ValueObjects;

using EducAIte.Domain.Enum;

public sealed record FlashcardAiEvaluationResult(
    decimal ConfidenceScore,
    FlashcardRiskLevel RiskLevel,
    string Insight,
    string ImprovementSuggestion,
    AiEvaluationStatus Status);
