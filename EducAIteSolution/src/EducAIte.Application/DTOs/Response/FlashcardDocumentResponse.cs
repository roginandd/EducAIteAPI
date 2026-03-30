namespace EducAIte.Application.DTOs.Response;

public record FlashcardDocumentResponse
{
    public required string Sqid { get; init; } = string.Empty;

    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string Name { get; init; } = string.Empty;

    public int FlashcardCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
