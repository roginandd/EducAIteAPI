namespace EducAIte.Application.DTOs.Response;

public record FlashcardSessionResponse
{
    public required string SessionSqid { get; init; } = string.Empty;

    public string? StudentCourseSqid { get; init; }

    public string? DocumentSqid { get; init; }

    public string ScopeType { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public int CurrentItemIndex { get; init; }

    public DateTime StartedAt { get; init; }

    public DateTime LastActiveAt { get; init; }

    public IReadOnlyList<FlashcardSessionItemResponse> Items { get; init; } = [];
}
