using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Enum;

namespace EducAIte.Application.Services.Interface;

public interface IFlashcardSessionService
{
    Task<FlashcardSessionResponse> StartSessionAsync(StartFlashcardSessionRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<FlashcardSessionResponse?> GetActiveSessionAsync(
        FlashcardSessionScopeType scopeType,
        string? studentCourseSqid,
        string? documentSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<FlashcardSessionResponse> ResumeSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default);

    Task<SubmitFlashcardSessionAnswerResponse> SubmitAnswerAsync(
        string sessionSqid,
        SubmitFlashcardSessionAnswerRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<SubmitEvaluatedFlashcardSessionAnswerResponse> SubmitEvaluatedAnswerAsync(
        string sessionSqid,
        SubmitEvaluatedFlashcardSessionAnswerRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<FlashcardSessionResponse> RestartSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default);

    Task AbandonSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default);
}
