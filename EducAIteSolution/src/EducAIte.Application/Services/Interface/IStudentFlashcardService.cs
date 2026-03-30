using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IStudentFlashcardService
{
    Task<StudentFlashcardProgressResponse> StartTrackingAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardProgressResponse?> GetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardReviewItemResponse>> GetReviewQueueAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardReviewItemResponse>> GetNextBatchAsync(long studentId, GetFlashcardReviewBatchRequest request, CancellationToken cancellationToken = default);

    Task<FlashcardAttemptResultResponse> SubmitAttemptAsync(string flashcardSqid, SubmitFlashcardAttemptRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<SubmitEvaluatedFlashcardAttemptResponse> SubmitEvaluatedAttemptAsync(
        string flashcardSqid,
        SubmitEvaluatedFlashcardAttemptRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<StudentFlashcardProgressResponse> RecordCorrectAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardProgressResponse> RecordWrongAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<StudentFlashcardProgressResponse> ResetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task ArchiveProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);
}
