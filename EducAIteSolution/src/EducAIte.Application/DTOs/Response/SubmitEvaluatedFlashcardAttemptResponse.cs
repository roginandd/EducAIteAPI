namespace EducAIte.Application.DTOs.Response;

public record SubmitEvaluatedFlashcardAttemptResponse
{
    public FlashcardAttemptResultResponse Attempt { get; init; } = null!;

    public StudentFlashcardAnalyticsResponse Analytics { get; init; } = null!;

    public string AnalyticsStatus { get; init; } = string.Empty;
}
