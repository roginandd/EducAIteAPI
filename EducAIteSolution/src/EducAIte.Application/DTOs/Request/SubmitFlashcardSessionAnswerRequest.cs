namespace EducAIte.Application.DTOs.Request;

public record SubmitFlashcardSessionAnswerRequest
{
    public required string SessionItemSqid { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public int ResponseTimeMs { get; init; }
}
