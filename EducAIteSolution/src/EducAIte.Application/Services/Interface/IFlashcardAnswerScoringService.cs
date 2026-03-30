using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IFlashcardAnswerScoringService
{
    Task<FlashcardAnswerScoringResult> ScoreAsync(
        string submittedAnswer,
        string expectedAnswer,
        int responseTimeMs,
        CancellationToken cancellationToken = default);
}
