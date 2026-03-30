namespace EducAIte.Application.DTOs.Request;

public record SubmitEvaluatedFlashcardSessionAnswerRequest
{
    public required string SessionItemSqid { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public int ResponseTimeMs { get; init; }

    public FlashcardAttemptEvaluationRequest Evaluation { get; init; } = null!;

    public UpsertFlashcardAnalyticsAiRequest Analytics { get; init; } = null!;
}
