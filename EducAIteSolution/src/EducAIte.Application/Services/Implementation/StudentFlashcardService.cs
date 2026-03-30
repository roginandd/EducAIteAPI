using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.Flashcard;
using EducAIte.Domain.Exceptions.StudentFlashcard;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class StudentFlashcardService : IStudentFlashcardService
{
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IStudentFlashcardAnalyticsService _studentFlashcardAnalyticsService;
    private readonly IFlashcardAnswerHistoryRepository _flashcardAnswerHistoryRepository;
    private readonly IPerformanceSummaryService _performanceSummaryService;
    private readonly IStudentPerformanceAiWorkQueue _studentPerformanceAiWorkQueue;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISqidService _sqidService;
    private readonly ILogger<StudentFlashcardService> _logger;

    public StudentFlashcardService(
        IStudentFlashcardRepository studentFlashcardRepository,
        IStudentFlashcardAnalyticsService studentFlashcardAnalyticsService,
        IFlashcardAnswerHistoryRepository flashcardAnswerHistoryRepository,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiWorkQueue studentPerformanceAiWorkQueue,
        IFlashcardRepository flashcardRepository,
        IUnitOfWork unitOfWork,
        ISqidService sqidService,
        ILogger<StudentFlashcardService> logger)
    {
        _studentFlashcardRepository = studentFlashcardRepository;
        _studentFlashcardAnalyticsService = studentFlashcardAnalyticsService;
        _flashcardAnswerHistoryRepository = flashcardAnswerHistoryRepository;
        _performanceSummaryService = performanceSummaryService;
        _studentPerformanceAiWorkQueue = studentPerformanceAiWorkQueue;
        _flashcardRepository = flashcardRepository;
        _unitOfWork = unitOfWork;
        _sqidService = sqidService;
        _logger = logger;
    }

    public async Task<StudentFlashcardProgressResponse> StartTrackingAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));
        DateTime now = DateTime.UtcNow;

        Flashcard flashcard = await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? existing = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (existing is not null)
        {
            await _studentFlashcardAnalyticsService.EnsureInitializedAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existing.ToProgressResponse(_sqidService);
        }

        StudentFlashcard? archived = await _studentFlashcardRepository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (archived is not null)
        {
            archived.Restore(now);
            archived.StartTracking(now);
            await _studentFlashcardRepository.UpdateAsync(archived, cancellationToken);
            await _studentFlashcardAnalyticsService.EnsureInitializedAsync(archived, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Restored flashcard progress for flashcard {FlashcardId}", flashcardId);
            return archived.ToProgressResponse(flashcard, _sqidService);
        }

        StudentFlashcard created = new(studentId, flashcardId);
        created.StartTracking(now);
        await _studentFlashcardRepository.AddAsync(created, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _studentFlashcardAnalyticsService.EnsureInitializedAsync(created, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Started flashcard progress for flashcard {FlashcardId}", flashcardId);

        return created.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse?> GetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? studentFlashcard = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        return studentFlashcard?.ToProgressResponse(_sqidService);
    }

    public async Task<IReadOnlyList<FlashcardReviewItemResponse>> GetReviewQueueAsync(long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        IReadOnlyList<StudentFlashcard> reviewQueue = await _studentFlashcardRepository.GetReviewQueueByStudentIdAsync(studentId, cancellationToken);
        return reviewQueue
            .Select(studentFlashcard => studentFlashcard.ToReviewItemResponse(_sqidService))
            .ToList();
    }

    public async Task<IReadOnlyList<FlashcardReviewItemResponse>> GetNextBatchAsync(
        long studentId,
        GetFlashcardReviewBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        ArgumentNullException.ThrowIfNull(request);

        int take = NormalizeBatchSize(request.Take);
        DateTime now = DateTime.UtcNow;
        HashSet<long> excludedFlashcardIds = DecodeSqids(request.ExcludeFlashcardSqids ?? Array.Empty<string>());

        IReadOnlyList<StudentFlashcard> dueBatch = await _studentFlashcardRepository.GetDueBatchByStudentIdAsync(
            studentId,
            now,
            excludedFlashcardIds,
            take,
            cancellationToken);

        List<FlashcardReviewItemResponse> items = dueBatch
            .Select(studentFlashcard => studentFlashcard.ToReviewItemResponse(_sqidService))
            .ToList();

        if (items.Count == take)
        {
            return items;
        }

        foreach (StudentFlashcard dueItem in dueBatch)
        {
            excludedFlashcardIds.Add(dueItem.FlashcardId);
        }

        IReadOnlyList<Flashcard> newFlashcards = await _flashcardRepository.GetUntrackedByStudentIdAsync(
            studentId,
            excludedFlashcardIds,
            take - items.Count,
            cancellationToken);

        items.AddRange(newFlashcards.Select(flashcard => flashcard.ToReviewItemResponse(_sqidService, now)));
        return items;
    }

    public async Task<FlashcardAttemptResultResponse> SubmitAttemptAsync(
        string flashcardSqid,
        SubmitFlashcardAttemptRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);

        string submittedAnswer = request.Answer?.Trim() ?? string.Empty;
        bool isCorrect = AnswersMatch(submittedAnswer, flashcard.Answer);

        studentFlashcard.ApplyReviewResult(isCorrect, DateTime.UtcNow);
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FlashcardAttemptResultResponse
        {
            FlashcardSqid = flashcardSqid,
            SubmittedAnswer = submittedAnswer,
            ExpectedAnswer = flashcard.Answer,
            Feedback = isCorrect
                ? "Correct. The flashcard will come back later."
                : "Incorrect. Review the expected answer and try it again later in the session.",
            IsCorrect = isCorrect,
            Evaluation = CreateEvaluationResponse(
                isCorrect ? FlashcardAnswerVerdict.ExactCorrect : FlashcardAnswerVerdict.Incorrect,
                isCorrect,
                isCorrect ? 5 : 0,
                isCorrect
                    ? "Correct. The flashcard will come back later."
                    : "Incorrect. Review the expected answer and try it again later in the session.",
                string.Empty),
            ShowAgainInSession = !isCorrect,
            RequeueAfter = isCorrect ? 0 : 3,
            NextReviewAt = studentFlashcard.NextReviewAt,
            Progress = studentFlashcard.ToProgressResponse(flashcard, _sqidService)
        };
    }

    public async Task<SubmitEvaluatedFlashcardAttemptResponse> SubmitEvaluatedAttemptAsync(
        string flashcardSqid,
        SubmitEvaluatedFlashcardAttemptRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Evaluation);
        ArgumentNullException.ThrowIfNull(request.Analytics);

        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);

        string submittedAnswer = request.Answer?.Trim() ?? string.Empty;
        DateTime now = DateTime.UtcNow;
        FlashcardAnswerVerdict verdict = ParseEnum<FlashcardAnswerVerdict>(request.Evaluation.Verdict, nameof(request.Evaluation.Verdict));

        studentFlashcard.ApplyEvaluation(
            request.Evaluation.AcceptedAsCorrect,
            request.Evaluation.QualityScore,
            verdict,
            now);

        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);

        FlashcardAnswerHistory history = FlashcardAnswerHistory.Create(
            flashcardSessionId: null,
            flashcardSessionItemId: null,
            studentFlashcardId: studentFlashcard.StudentFlashcardId,
            submittedAnswer: submittedAnswer,
            expectedAnswerSnapshot: flashcard.Answer,
            responseTimeMs: request.ResponseTimeMs,
            aiQualityScore: request.Evaluation.QualityScore,
            fallbackQualityScore: null,
            finalQualityScore: request.Evaluation.QualityScore,
            wasAcceptedAsCorrect: request.Evaluation.AcceptedAsCorrect,
            scoringSource: AnswerScoringSource.Ai,
            answeredAt: now);

        history.AttachEvaluation(new FlashcardAnswerEvaluation(
            verdict,
            request.Evaluation.AcceptedAsCorrect,
            request.Evaluation.QualityScore,
            request.Evaluation.FeedbackSummary,
            request.Evaluation.SemanticRationale));

        await _flashcardAnswerHistoryRepository.AddAsync(history, cancellationToken);

        IReadOnlyList<FlashcardAnswerHistory> existingRecentAnswers =
            await _flashcardAnswerHistoryRepository.GetRecentByStudentFlashcardIdAsync(studentFlashcard.StudentFlashcardId, 9, cancellationToken);

        List<FlashcardAnswerHistory> analyticsRecentAnswers = [history, .. existingRecentAnswers];
        await _studentFlashcardAnalyticsService.ApplyEvaluatedAttemptAnalyticsAsync(
            studentFlashcard,
            request.Analytics,
            analyticsRecentAnswers,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        long studentCourseId = flashcard.Note.Document.Folder.StudentCourseId;
        await _performanceSummaryService.RefreshStudentCourseSummaryAsync(studentCourseId, cancellationToken);
        await _performanceSummaryService.RefreshOverallSummaryAsync(studentId, cancellationToken);
        await _studentPerformanceAiWorkQueue.QueueAsync(
            new StudentPerformanceAiWorkItem(studentId, _sqidService.Encode(studentCourseId)),
            cancellationToken);

        StudentFlashcardAnalyticsResponse analytics =
            await _studentFlashcardAnalyticsService.GetByFlashcardSqidAsync(flashcardSqid, studentId, cancellationToken)
            ?? throw new StudentFlashcardValidationException("Unable to load flashcard analytics after the evaluated attempt was saved.");

        FlashcardAttemptResultResponse attempt = new()
        {
            FlashcardSqid = flashcardSqid,
            SubmittedAnswer = submittedAnswer,
            ExpectedAnswer = flashcard.Answer,
            Feedback = request.Evaluation.FeedbackSummary,
            IsCorrect = request.Evaluation.AcceptedAsCorrect,
            Evaluation = CreateEvaluationResponse(
                verdict,
                request.Evaluation.AcceptedAsCorrect,
                request.Evaluation.QualityScore,
                request.Evaluation.FeedbackSummary,
                request.Evaluation.SemanticRationale),
            ShowAgainInSession = !request.Evaluation.AcceptedAsCorrect,
            RequeueAfter = request.Evaluation.AcceptedAsCorrect ? 0 : 3,
            NextReviewAt = studentFlashcard.NextReviewAt,
            Progress = studentFlashcard.ToProgressResponse(flashcard, _sqidService)
        };

        return new SubmitEvaluatedFlashcardAttemptResponse
        {
            Attempt = attempt,
            Analytics = analytics,
            AnalyticsStatus = analytics.AiStatus
        };
    }

    public async Task<StudentFlashcardProgressResponse> RecordCorrectAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.MarkCorrect();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse> RecordWrongAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.MarkWrong();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse> ResetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.ResetProgress();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task ArchiveProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? studentFlashcard = await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (studentFlashcard is null)
        {
            return;
        }

        studentFlashcard.MarkDeleted();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Archived flashcard progress for flashcard {FlashcardId}", flashcardId);
    }

    private async Task<(StudentFlashcard StudentFlashcard, Flashcard Flashcard)> EnsureTrackedProgressAsync(
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        Flashcard flashcard = await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? active = await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (active is not null)
        {
            await _studentFlashcardAnalyticsService.EnsureInitializedAsync(active, cancellationToken);
            return (active, flashcard);
        }

        StudentFlashcard? archived = await _studentFlashcardRepository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (archived is not null)
        {
            archived.Restore(DateTime.UtcNow);
            await _studentFlashcardRepository.UpdateAsync(archived, cancellationToken);
            await _studentFlashcardAnalyticsService.EnsureInitializedAsync(archived, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (archived, flashcard);
        }

        StudentFlashcard created = new(studentId, flashcardId);
        created.StartTracking(DateTime.UtcNow);
        await _studentFlashcardRepository.AddAsync(created, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _studentFlashcardAnalyticsService.EnsureInitializedAsync(created, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (created, flashcard);
    }

    private async Task<Flashcard> GetOwnedFlashcardOrThrowAsync(long flashcardId, long studentId, CancellationToken cancellationToken)
    {
        Flashcard? flashcard = await _flashcardRepository.GetByIdAndStudentIdAsync(flashcardId, studentId, cancellationToken);
        if (flashcard is not null)
        {
            return flashcard;
        }

        bool exists = await _flashcardRepository.ExistsByIdAsync(flashcardId, cancellationToken);
        if (exists)
        {
            throw new FlashcardForbiddenException();
        }

        throw new FlashcardNotFoundException(flashcardId);
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new InvalidSqidException(fieldName);
        }

        return id;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new StudentFlashcardValidationException("StudentId must be greater than zero.");
        }
    }

    private HashSet<long> DecodeSqids(IReadOnlyCollection<string> flashcardSqids)
    {
        HashSet<long> decoded = new();

        foreach (string flashcardSqid in flashcardSqids)
        {
            if (string.IsNullOrWhiteSpace(flashcardSqid))
            {
                continue;
            }

            decoded.Add(DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid)));
        }

        return decoded;
    }

    private static int NormalizeBatchSize(int take)
    {
        if (take <= 0)
        {
            return 30;
        }

        return Math.Min(take, 100);
    }

    private static bool AnswersMatch(string submittedAnswer, string expectedAnswer)
    {
        return NormalizeForComparison(submittedAnswer) == NormalizeForComparison(expectedAnswer);
    }

    private static FlashcardAnswerEvaluationResponse CreateEvaluationResponse(
        FlashcardAnswerVerdict verdict,
        bool acceptedAsCorrect,
        int qualityScore,
        string feedbackSummary,
        string semanticRationale)
    {
        return new FlashcardAnswerEvaluationResponse
        {
            Verdict = verdict.ToString(),
            AcceptedAsCorrect = acceptedAsCorrect,
            QualityScore = qualityScore,
            FeedbackSummary = feedbackSummary,
            SemanticRationale = semanticRationale
        };
    }

    private static TEnum ParseEnum<TEnum>(string rawValue, string fieldName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(rawValue) || !Enum.TryParse(rawValue.Trim(), true, out TEnum parsedValue))
        {
            throw new StudentFlashcardValidationException($"{fieldName} is invalid.");
        }

        return parsedValue;
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
