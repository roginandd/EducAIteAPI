namespace EducAIte.Application.DTOs.Response;

public record StudentFlashcardProgressResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string DocumentSqid { get; init; } = string.Empty;

    public int CorrectCount { get; init; }

    public int WrongCount { get; init; }

    public int TotalAttempts { get; init; }

    public int ConsecutiveCorrectCount { get; init; }

    public int ReviewCount { get; init; }

    public int LapseCount { get; init; }

    public string ReviewState { get; init; } = string.Empty;

    public string? LastReviewOutcome { get; init; }

    public DateTime? LastReviewedAt { get; init; }

    public DateTime NextReviewAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
