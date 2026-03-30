using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Interfaces;
using EducAIte.Domain.ValueObjects;
using EducAIte.Domain.Exceptions.StudentFlashcard;

namespace EducAIte.Application.Services.Implementation;

public sealed class StudentFlashcardAnalyticsService : IStudentFlashcardAnalyticsService
{
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IStudentFlashcardAnalyticsRepository _studentFlashcardAnalyticsRepository;
    private readonly IFlashcardAnswerHistoryRepository _flashcardAnswerHistoryRepository;
    private readonly IPerformanceSummaryService _performanceSummaryService;
    private readonly IStudentPerformanceAiWorkQueue _studentPerformanceAiWorkQueue;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;

    public StudentFlashcardAnalyticsService(
        IStudentFlashcardRepository studentFlashcardRepository,
        IStudentFlashcardAnalyticsRepository studentFlashcardAnalyticsRepository,
        IFlashcardAnswerHistoryRepository flashcardAnswerHistoryRepository,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiWorkQueue studentPerformanceAiWorkQueue,
        ISqidService sqidService,
        IUnitOfWork unitOfWork)
    {
        _studentFlashcardRepository = studentFlashcardRepository;
        _studentFlashcardAnalyticsRepository = studentFlashcardAnalyticsRepository;
        _flashcardAnswerHistoryRepository = flashcardAnswerHistoryRepository;
        _performanceSummaryService = performanceSummaryService;
        _studentPerformanceAiWorkQueue = studentPerformanceAiWorkQueue;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentFlashcardAnalyticsResponse?> GetByFlashcardSqidAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));
        StudentFlashcard? progress = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (progress is null)
        {
            return null;
        }

        StudentFlashcardAnalytics? analytics = await _studentFlashcardAnalyticsRepository.GetByStudentFlashcardIdAsync(progress.StudentFlashcardId, cancellationToken);
        if (analytics is null)
        {
            return null;
        }

        return ToResponse(progress, analytics);
    }

    public async Task<StudentFlashcardAnalytics> EnsureInitializedAsync(StudentFlashcard progress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);

        StudentFlashcardAnalytics? existingAnalytics =
            await _studentFlashcardAnalyticsRepository.GetTrackedByStudentFlashcardIdAsync(progress.StudentFlashcardId, cancellationToken);

        if (existingAnalytics is not null)
        {
            return existingAnalytics;
        }

        if (progress.TotalAttempts > 0 || progress.ReviewCount > 0 || progress.LastReviewedAt.HasValue || progress.LapseCount > 0)
        {
            return await RecomputeAsync(progress, cancellationToken);
        }

        StudentFlashcardAnalytics analytics = new(progress.StudentFlashcardId);
        analytics.InitializeDefaults(progress, DateTime.UtcNow);
        await _studentFlashcardAnalyticsRepository.AddAsync(analytics, cancellationToken);
        return analytics;
    }

    public async Task<FlashcardAnalyticsEvaluationContextResponse?> GetEvaluationContextAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));
        StudentFlashcard? progress = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (progress is null)
        {
            return null;
        }

        IReadOnlyList<FlashcardAnswerHistory> recentAnswers =
            await _flashcardAnswerHistoryRepository.GetRecentByStudentFlashcardIdAsync(progress.StudentFlashcardId, 10, cancellationToken);

        StudentFlashcardAnalytics? analytics =
            await _studentFlashcardAnalyticsRepository.GetByStudentFlashcardIdAsync(progress.StudentFlashcardId, cancellationToken);

        Flashcard flashcard = progress.Flashcard;

        return new FlashcardAnalyticsEvaluationContextResponse
        {
            FlashcardSqid = _sqidService.Encode(flashcard.FlashcardId),
            StudentCourseSqid = _sqidService.Encode(flashcard.Note.Document.Folder.StudentCourseId),
            Question = flashcard.Question,
            ExpectedAnswer = flashcard.Answer,
            ConceptExplanation = flashcard.ConceptExplanation,
            AnsweringGuidance = flashcard.AnsweringGuidance,
            AcceptedAnswerAliases = flashcard.AcceptedAnswerAliases
                .OrderBy(alias => alias.Order)
                .Select(alias => alias.Alias)
                .ToList(),
            Progress = new FlashcardAnalyticsProgressSnapshotResponse
            {
                CorrectCount = progress.CorrectCount,
                WrongCount = progress.WrongCount,
                TotalAttempts = progress.TotalAttempts,
                ConsecutiveCorrectCount = progress.ConsecutiveCorrectCount,
                ConsecutiveWrongCount = progress.ConsecutiveWrongCount,
                ReviewCount = progress.ReviewCount,
                LapseCount = progress.LapseCount,
                LastReviewOutcome = progress.LastReviewOutcome?.ToString() ?? string.Empty,
                LastEvaluationVerdict = progress.LastEvaluationVerdict?.ToString(),
                LastQualityScore = progress.LastQualityScore,
                LastReviewedAt = progress.LastReviewedAt,
                NextReviewAt = progress.NextReviewAt
            },
            RecentAnswers = recentAnswers
                .Select(answer => new FlashcardAnalyticsRecentAnswerResponse
                {
                    SubmittedAnswer = answer.SubmittedAnswer,
                    ExpectedAnswerSnapshot = answer.ExpectedAnswerSnapshot,
                    ResponseTimeMs = answer.ResponseTimeMs,
                    FinalQualityScore = answer.FinalQualityScore,
                    WasAcceptedAsCorrect = answer.WasAcceptedAsCorrect,
                    Verdict = answer.Evaluation?.Verdict.ToString(),
                    FeedbackSummary = answer.Evaluation?.FeedbackSummary ?? string.Empty,
                    SemanticRationale = answer.Evaluation?.SemanticRationale ?? string.Empty,
                    AnsweredAt = answer.AnsweredAt
                })
                .ToList(),
            CurrentAnalytics = analytics is null ? null : ToResponse(progress, analytics)
        };
    }

    public async Task<StudentFlashcardAnalyticsResponse> ApplyAiEvaluationAsync(
        string flashcardSqid,
        UpsertFlashcardAnalyticsAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));
        StudentFlashcard progress =
            await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken)
            ?? throw new StudentFlashcardNotFoundException(flashcardId);

        IReadOnlyList<FlashcardAnswerHistory> recentAnswers =
            await _flashcardAnswerHistoryRepository.GetRecentByStudentFlashcardIdAsync(progress.StudentFlashcardId, 10, cancellationToken);

        StudentFlashcardAnalytics? existingAnalytics =
            await _studentFlashcardAnalyticsRepository.GetTrackedByStudentFlashcardIdAsync(progress.StudentFlashcardId, cancellationToken);

        StudentFlashcardAnalytics analytics = await ApplyEvaluatedAttemptAnalyticsAsync(progress, request, recentAnswers, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        long studentCourseId = progress.Flashcard.Note.Document.Folder.StudentCourseId;
        await _performanceSummaryService.RefreshStudentCourseSummaryAsync(studentCourseId, cancellationToken);
        await _performanceSummaryService.RefreshOverallSummaryAsync(studentId, cancellationToken);
        await _studentPerformanceAiWorkQueue.QueueAsync(
            new StudentPerformanceAiWorkItem(studentId, _sqidService.Encode(studentCourseId)),
            cancellationToken);

        return ToResponse(progress, analytics);
    }

    public async Task<StudentFlashcardAnalytics> ApplyEvaluatedAttemptAnalyticsAsync(
        StudentFlashcard progress,
        UpsertFlashcardAnalyticsAiRequest request,
        IReadOnlyList<FlashcardAnswerHistory> recentAnswers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(recentAnswers);

        StudentFlashcardAnalytics? existingAnalytics =
            await _studentFlashcardAnalyticsRepository.GetTrackedByStudentFlashcardIdAsync(progress.StudentFlashcardId, cancellationToken);

        StudentFlashcardAnalytics analytics = existingAnalytics ?? new StudentFlashcardAnalytics(progress.StudentFlashcardId);
        DateTime now = DateTime.UtcNow;

        analytics.RecomputeFrom(progress, recentAnswers, now);
        analytics.ApplyExternalEvaluation(CreateEvaluationResult(request, progress), now);

        if (existingAnalytics is null)
        {
            await _studentFlashcardAnalyticsRepository.AddAsync(analytics, cancellationToken);
        }
        else
        {
            await _studentFlashcardAnalyticsRepository.UpdateAsync(analytics, cancellationToken);
        }

        return analytics;
    }

    public async Task<StudentFlashcardAnalytics> RecomputeAsync(StudentFlashcard progress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);

        IReadOnlyList<FlashcardAnswerHistory> recentAnswers =
            await _flashcardAnswerHistoryRepository.GetRecentByStudentFlashcardIdAsync(progress.StudentFlashcardId, 10, cancellationToken);

        StudentFlashcardAnalytics analytics =
            await EnsureInitializedAsync(progress, cancellationToken);

        analytics.RecomputeFrom(progress, recentAnswers, DateTime.UtcNow);

        await _studentFlashcardAnalyticsRepository.UpdateAsync(analytics, cancellationToken);

        return analytics;
    }

    public async Task QueueAiEvaluationAsync(long studentFlashcardId, CancellationToken cancellationToken = default)
    {
        StudentFlashcardAnalytics? analytics = await _studentFlashcardAnalyticsRepository.GetTrackedByStudentFlashcardIdAsync(studentFlashcardId, cancellationToken);
        if (analytics is null)
        {
            return;
        }

        // The external educAIteAI agent is now the source of truth for AI-authored analytics.
        analytics.MarkAiPending(DateTime.UtcNow);
        await _studentFlashcardAnalyticsRepository.UpdateAsync(analytics, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private FlashcardAnalyticsAiEvaluationResult CreateEvaluationResult(UpsertFlashcardAnalyticsAiRequest request, StudentFlashcard progress)
    {
        DateTime normalizedNextReviewAt = request.NextReviewAt.ToUniversalTime();
        DateTime minimumNextReviewAt = progress.NextReviewAt.ToUniversalTime();

        // The AI evaluates against pre-persistence context, so its proposed schedule can lag
        // behind the just-applied review timestamp or backend-derived next review date.
        if (progress.LastReviewedAt.HasValue && progress.LastReviewedAt.Value.ToUniversalTime() > minimumNextReviewAt)
        {
            minimumNextReviewAt = progress.LastReviewedAt.Value.ToUniversalTime();
        }

        if (normalizedNextReviewAt < minimumNextReviewAt)
        {
            normalizedNextReviewAt = minimumNextReviewAt;
        }

        return new FlashcardAnalyticsAiEvaluationResult(
            normalizedNextReviewAt,
            request.EaseFactor,
            request.RepetitionCount,
            request.IntervalDays,
            request.LapseCount,
            ParseEnum<FlashcardMasteryLevel>(request.MasteryLevel, nameof(request.MasteryLevel)),
            request.ConfidenceScore,
            request.ConsistencyScore,
            request.RetentionScore,
            ParseEnum<FlashcardRiskLevel>(request.RiskLevel, nameof(request.RiskLevel)),
            request.AiInsight,
            request.ImprovementSuggestion,
            ParseEnum<AiEvaluationStatus>(request.AiStatus, nameof(request.AiStatus)));
    }

    private static TEnum ParseEnum<TEnum>(string rawValue, string fieldName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(rawValue) || !Enum.TryParse(rawValue.Trim(), true, out TEnum parsedValue))
        {
            string allowedValues = string.Join(", ", Enum.GetNames<TEnum>());
            throw new StudentFlashcardValidationException($"{fieldName} must be one of: {allowedValues}.");
        }

        return parsedValue;
    }

    private StudentFlashcardAnalyticsResponse ToResponse(StudentFlashcard progress, StudentFlashcardAnalytics analytics)
    {
        Flashcard flashcard = progress.Flashcard;

        return new StudentFlashcardAnalyticsResponse
        {
            FlashcardSqid = _sqidService.Encode(flashcard.FlashcardId),
            // Resolve the academic context from the flashcard ownership chain instead of duplicating it on StudentFlashcard.
            StudentCourseSqid = _sqidService.Encode(flashcard.Note.Document.Folder.StudentCourseId),
            LastAnsweredAt = analytics.LastAnsweredAt,
            NextReviewAt = analytics.NextReviewAt,
            EaseFactor = analytics.EaseFactor,
            RepetitionCount = analytics.RepetitionCount,
            IntervalDays = analytics.IntervalDays,
            LapseCount = analytics.LapseCount,
            MasteryLevel = analytics.MasteryLevel.ToString(),
            ConfidenceScore = analytics.ConfidenceScore,
            ConsistencyScore = analytics.ConsistencyScore,
            RetentionScore = analytics.RetentionScore,
            RiskLevel = analytics.RiskLevel.ToString(),
            AiStatus = analytics.AiStatus.ToString(),
            AiInsight = analytics.AiInsight,
            ImprovementSuggestion = analytics.ImprovementSuggestion,
            AiLastEvaluatedAt = analytics.AiLastEvaluatedAt,
            LastComputedAt = analytics.LastComputedAt
        };
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new InvalidSqidException(fieldName);
        }

        return id;
    }
}
