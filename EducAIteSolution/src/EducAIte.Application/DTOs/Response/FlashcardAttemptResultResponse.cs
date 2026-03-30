namespace EducAIte.Application.DTOs.Response;

public record FlashcardAttemptResultResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string SubmittedAnswer { get; init; } = string.Empty;

    public required string ExpectedAnswer { get; init; } = string.Empty;

    public required string Feedback { get; init; } = string.Empty;

    public bool IsCorrect { get; init; }

    public FlashcardAnswerEvaluationResponse Evaluation { get; init; } = null!;

    public bool ShowAgainInSession { get; init; }

    public int RequeueAfter { get; init; }

    public DateTime NextReviewAt { get; init; }

    public StudentFlashcardProgressResponse Progress { get; init; } = null!;
}

public record FlashcardAnswerEvaluationResponse
{
    public required string Verdict { get; init; } = string.Empty;

    public bool AcceptedAsCorrect { get; init; }

    public int QualityScore { get; init; }

    public required string FeedbackSummary { get; init; } = string.Empty;

    public string SemanticRationale { get; init; } = string.Empty;
}
