namespace EducAIte.Application.DTOs.Response;

public record FlashcardSessionItemResponse
{
    public required string SessionItemSqid { get; init; } = string.Empty;

    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string Question { get; init; } = string.Empty;

    public int OriginalOrder { get; init; }

    public int CurrentOrder { get; init; }

    public string Status { get; init; } = string.Empty;
}
