namespace EducAIte.Application.DTOs.Request;

public record SubmitEvaluatedFlashcardAttemptRequest
{
    public required string Answer { get; init; } = string.Empty;

    public int ResponseTimeMs { get; init; }

    public FlashcardAttemptEvaluationRequest Evaluation { get; init; } = null!;

    public UpsertFlashcardAnalyticsAiRequest Analytics { get; init; } = null!;
}

public record FlashcardAttemptEvaluationRequest
{
    public required string Verdict { get; init; } = string.Empty;

    public bool AcceptedAsCorrect { get; init; }

    public int QualityScore { get; init; }

    public required string FeedbackSummary { get; init; } = string.Empty;

    public string SemanticRationale { get; init; } = string.Empty;
}
