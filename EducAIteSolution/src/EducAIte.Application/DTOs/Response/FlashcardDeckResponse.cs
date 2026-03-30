namespace EducAIte.Application.DTOs.Response;

public record FlashcardDeckResponse
{
    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string DeckName { get; init; } = string.Empty;

    public required string EDPCode { get; init; } = string.Empty;

    public int DocumentCount { get; init; }

    public int FlashcardCount { get; init; }
}
