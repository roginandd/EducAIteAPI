using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Response;

public record FlashcardAnswerScoringResult
{
    public int? AiQualityScore { get; init; }

    public int? FallbackQualityScore { get; init; }

    public int FinalQualityScore { get; init; }

    public bool WasAcceptedAsCorrect { get; init; }

    public AnswerScoringSource ScoringSource { get; init; }

    public string Feedback { get; init; } = string.Empty;

    public string SuggestedAction { get; init; } = string.Empty;
}
