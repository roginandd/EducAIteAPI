namespace EducAIte.Application.DTOs.Request;

public record SubmitFlashcardAttemptRequest
{
    public required string Answer { get; init; } = string.Empty;
}
