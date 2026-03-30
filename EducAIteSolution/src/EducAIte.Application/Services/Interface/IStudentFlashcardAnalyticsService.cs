using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;

namespace EducAIte.Application.Services.Interface;

public interface IStudentFlashcardAnalyticsService
{
    Task<StudentFlashcardAnalyticsResponse?> GetByFlashcardSqidAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardAnalytics> EnsureInitializedAsync(StudentFlashcard progress, CancellationToken cancellationToken = default);

    Task<FlashcardAnalyticsEvaluationContextResponse?> GetEvaluationContextAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardAnalyticsResponse> ApplyAiEvaluationAsync(
        string flashcardSqid,
        UpsertFlashcardAnalyticsAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentFlashcardAnalytics> ApplyEvaluatedAttemptAnalyticsAsync(
        StudentFlashcard progress,
        UpsertFlashcardAnalyticsAiRequest request,
        IReadOnlyList<FlashcardAnswerHistory> recentAnswers,
        CancellationToken cancellationToken = default);

    Task<StudentFlashcardAnalytics> RecomputeAsync(StudentFlashcard progress, CancellationToken cancellationToken = default);

    Task QueueAiEvaluationAsync(long studentFlashcardId, CancellationToken cancellationToken = default);
}
