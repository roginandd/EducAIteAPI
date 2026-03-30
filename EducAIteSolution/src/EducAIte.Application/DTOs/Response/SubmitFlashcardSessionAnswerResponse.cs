namespace EducAIte.Application.DTOs.Response;

public record SubmitFlashcardSessionAnswerResponse
{
    public required string SessionSqid { get; init; } = string.Empty;

    public required string SessionItemSqid { get; init; } = string.Empty;

    public int QualityScore { get; init; }

    public bool ShowAgainInSession { get; init; }

    public int? RequeuedToOrder { get; init; }

    public DateTime NextReviewAt { get; init; }

    public StudentFlashcardProgressResponse Progress { get; init; } = null!;
}
