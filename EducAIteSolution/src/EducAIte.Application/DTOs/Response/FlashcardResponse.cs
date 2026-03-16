namespace EducAIte.Application.DTOs.Response;

public record FlashcardResponse
{
    public required string Sqid { get; init; } = string.Empty;

    public required string Question { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public required string NoteSqid { get; init; } = string.Empty;

    public required string DocumentSqid { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
