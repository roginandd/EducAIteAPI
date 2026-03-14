namespace EducAIte.Application.DTOs.Response;

public record FlashcardReviewItemResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string DocumentSqid { get; init; } = string.Empty;

    public required string Question { get; init; } = string.Empty;

    public int CorrectCount { get; init; }

    public int WrongCount { get; init; }

    public int TotalAttempts { get; init; }

    public bool IsTracked { get; init; }

    public string ReviewState { get; init; } = string.Empty;

    public DateTime? LastReviewedAt { get; init; }

    public DateTime NextReviewAt { get; init; }
}
