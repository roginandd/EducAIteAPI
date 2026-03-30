namespace EducAIte.Application.DTOs.Response;

public record SubmitEvaluatedFlashcardSessionAnswerResponse
{
    public FlashcardSessionResponse Session { get; init; } = null!;

    public FlashcardSessionAnswerEvaluationResultResponse Answer { get; init; } = null!;
}

public record FlashcardSessionAnswerEvaluationResultResponse
{
    public required string SessionItemSqid { get; init; } = string.Empty;

    public required string FlashcardSqid { get; init; } = string.Empty;

    public int QualityScore { get; init; }

    public bool ShowAgainInSession { get; init; }

    public int? RequeuedToOrder { get; init; }

    public DateTime NextReviewAt { get; init; }

    public StudentFlashcardProgressResponse Progress { get; init; } = null!;

    public FlashcardAnswerEvaluationResponse Evaluation { get; init; } = null!;

    public StudentFlashcardAnalyticsResponse Analytics { get; init; } = null!;
}
