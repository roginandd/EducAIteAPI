using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Enum;

namespace EducAIte.Application.Services.Implementation;

// This implementation keeps the scoring contract AI-ready while using a deterministic fallback today.
public sealed class FlashcardAnswerScoringService : IFlashcardAnswerScoringService
{
    public Task<FlashcardAnswerScoringResult> ScoreAsync(
        string submittedAnswer,
        string expectedAnswer,
        int responseTimeMs,
        CancellationToken cancellationToken = default)
    {
        string normalizedSubmitted = NormalizeForComparison(submittedAnswer);
        string normalizedExpected = NormalizeForComparison(expectedAnswer);

        int qualityScore;
        bool isCorrectLike;

        if (normalizedSubmitted.Length == 0)
        {
            qualityScore = 0;
            isCorrectLike = false;
        }
        else if (normalizedSubmitted == normalizedExpected)
        {
            qualityScore = responseTimeMs <= 2500 ? 5 : 4;
            isCorrectLike = true;
        }
        else if (normalizedExpected.Contains(normalizedSubmitted, StringComparison.Ordinal) ||
                 normalizedSubmitted.Contains(normalizedExpected, StringComparison.Ordinal))
        {
            qualityScore = 3;
            isCorrectLike = true;
        }
        else
        {
            qualityScore = responseTimeMs <= 1500 ? 1 : 0;
            isCorrectLike = false;
        }

        return Task.FromResult(new FlashcardAnswerScoringResult
        {
            FallbackQualityScore = qualityScore,
            FinalQualityScore = qualityScore,
            WasAcceptedAsCorrect = isCorrectLike,
            ScoringSource = AnswerScoringSource.FallbackRules,
            Feedback = isCorrectLike
                ? "Accepted. The next review interval was updated based on the answer quality."
                : "Incorrect. The flashcard was scheduled sooner so it can be revisited.",
            SuggestedAction = isCorrectLike
                ? "Keep reinforcing the recall without relying on hints."
                : "Slow down and restate the answer before submitting to reduce rushed mistakes."
        });
    }

    private static string NormalizeForComparison(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
                ' ',
                value.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToUpperInvariant();
    }
}
