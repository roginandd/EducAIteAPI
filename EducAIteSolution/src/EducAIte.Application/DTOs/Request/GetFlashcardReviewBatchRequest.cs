namespace EducAIte.Application.DTOs.Request;

public record GetFlashcardReviewBatchRequest
{
    public int Take { get; init; } = 30;

    public IReadOnlyCollection<string> ExcludeFlashcardSqids { get; init; } = Array.Empty<string>();
}
