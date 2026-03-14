namespace EducAIte.Application.DTOs.Request;

public record UpdateFlashcardRequest
{
    public required string Question { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public required string DocumentSqid { get; init; } = string.Empty;
}
