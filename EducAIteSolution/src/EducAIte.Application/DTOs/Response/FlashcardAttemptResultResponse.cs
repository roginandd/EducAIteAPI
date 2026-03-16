namespace EducAIte.Application.DTOs.Response;

public record FlashcardAttemptResultResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string SubmittedAnswer { get; init; } = string.Empty;

    public required string ExpectedAnswer { get; init; } = string.Empty;

    public required string Feedback { get; init; } = string.Empty;

    public bool IsCorrect { get; init; }

    public bool ShowAgainInSession { get; init; }

    public int RequeueAfter { get; init; }

    public DateTime NextReviewAt { get; init; }

    public StudentFlashcardProgressResponse Progress { get; init; } = null!;
}
