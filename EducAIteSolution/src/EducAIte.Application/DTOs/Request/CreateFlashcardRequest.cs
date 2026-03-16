namespace EducAIte.Application.DTOs.Request;

public record CreateFlashcardRequest
{
    public required string Question { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public required string NoteSqid { get; init; } = string.Empty;
}
