namespace EducAIte.Application.DTOs.Response;

public record FlashcardWorkspaceLatestResponse
{
    public required string LatestGroupLabel { get; init; } = string.Empty;

    public int SchoolYearStart { get; init; }

    public int SchoolYearEnd { get; init; }

    public int Semester { get; init; }

    public IReadOnlyList<FlashcardDeckResponse> Decks { get; init; } = [];
}
