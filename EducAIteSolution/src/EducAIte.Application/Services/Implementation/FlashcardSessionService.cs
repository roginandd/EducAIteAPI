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
using EducAIte.Domain.Exceptions.StudentCourse;
using EducAIte.Domain.Exceptions.StudentFlashcard;
using EducAIte.Domain.Interfaces;

namespace EducAIte.Application.Services.Implementation;

public sealed class FlashcardSessionService : IFlashcardSessionService
{
    private readonly IFlashcardSessionRepository _flashcardSessionRepository;
    private readonly IFlashcardSessionItemRepository _flashcardSessionItemRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IStudentFlashcardAnalyticsRepository _studentFlashcardAnalyticsRepository;
    private readonly IFlashcardAnswerHistoryRepository _flashcardAnswerHistoryRepository;
    private readonly IFlashcardAnswerScoringService _flashcardAnswerScoringService;
    private readonly IStudentFlashcardAnalyticsService _studentFlashcardAnalyticsService;
    private readonly IPerformanceSummaryService _performanceSummaryService;
    private readonly IStudentPerformanceAiWorkQueue _studentPerformanceAiWorkQueue;
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISqidService _sqidService;

    public FlashcardSessionService(
        IFlashcardSessionRepository flashcardSessionRepository,
        IFlashcardSessionItemRepository flashcardSessionItemRepository,
        IFlashcardRepository flashcardRepository,
        IStudentFlashcardRepository studentFlashcardRepository,
        IStudentFlashcardAnalyticsRepository studentFlashcardAnalyticsRepository,
        IFlashcardAnswerHistoryRepository flashcardAnswerHistoryRepository,
        IFlashcardAnswerScoringService flashcardAnswerScoringService,
        IStudentFlashcardAnalyticsService studentFlashcardAnalyticsService,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiWorkQueue studentPerformanceAiWorkQueue,
        IStudentCourseRepository studentCourseRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ISqidService sqidService)
    {
        _flashcardSessionRepository = flashcardSessionRepository;
        _flashcardSessionItemRepository = flashcardSessionItemRepository;
        _flashcardRepository = flashcardRepository;
        _studentFlashcardRepository = studentFlashcardRepository;
        _studentFlashcardAnalyticsRepository = studentFlashcardAnalyticsRepository;
        _flashcardAnswerHistoryRepository = flashcardAnswerHistoryRepository;
        _flashcardAnswerScoringService = flashcardAnswerScoringService;
        _studentFlashcardAnalyticsService = studentFlashcardAnalyticsService;
        _performanceSummaryService = performanceSummaryService;
        _studentPerformanceAiWorkQueue = studentPerformanceAiWorkQueue;
        _studentCourseRepository = studentCourseRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _sqidService = sqidService;
    }

    public async Task<FlashcardSessionResponse> StartSessionAsync(StartFlashcardSessionRequest request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        long? studentCourseId = await ResolveStudentCourseIdAsync(request.ScopeType, request.StudentCourseSqid, studentId, cancellationToken);
        long? documentId = await ResolveDocumentIdAsync(request.DocumentSqid, studentId, studentCourseId, cancellationToken);
        FlashcardSession? activeSession = await FindMatchingActiveSessionAsync(
            studentId,
            studentCourseId,
            request.ScopeType,
            documentId,
            cancellationToken);

        if (activeSession is not null)
        {
            return await ResumeSessionAsync(_sqidService.Encode(activeSession.FlashcardSessionId), studentId, cancellationToken);
        }

        IReadOnlyList<StudentFlashcardAnalytics> candidates = documentId.HasValue
            ? await GetDocumentCandidatesAsync(studentId, documentId.Value, NormalizeBatchSize(request.Take), cancellationToken)
            : await _studentFlashcardAnalyticsRepository.GetPrioritizedCandidatesAsync(
                studentId,
                studentCourseId,
                DateTime.UtcNow,
                NormalizeBatchSize(request.Take),
                cancellationToken);

        FlashcardSession session = new(studentId, studentCourseId, request.ScopeType);
        session.Start(DateTime.UtcNow);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _flashcardSessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            List<FlashcardSessionItem> items = candidates
                .Select((candidate, index) => new FlashcardSessionItem(session.FlashcardSessionId, candidate.StudentFlashcardId, index))
                .ToList();

            await _flashcardSessionItemRepository.AddRangeAsync(items, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return await ResumeSessionAsync(_sqidService.Encode(session.FlashcardSessionId), studentId, cancellationToken);
    }

    public async Task<FlashcardSessionResponse?> GetActiveSessionAsync(
        FlashcardSessionScopeType scopeType,
        string? studentCourseSqid,
        string? documentSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        long? studentCourseId = await ResolveStudentCourseIdAsync(scopeType, studentCourseSqid, studentId, cancellationToken);
        long? documentId = await ResolveDocumentIdAsync(documentSqid, studentId, studentCourseId, cancellationToken);
        FlashcardSession? session = await FindMatchingActiveSessionAsync(
            studentId,
            studentCourseId,
            scopeType,
            documentId,
            cancellationToken);

        return session is null ? null : await ResumeSessionAsync(_sqidService.Encode(session.FlashcardSessionId), studentId, cancellationToken);
    }

    public async Task<FlashcardSessionResponse> ResumeSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long sessionId = DecodeRequiredSqid(sessionSqid, nameof(sessionSqid));
        FlashcardSession session = await GetSessionOrThrowAsync(sessionId, studentId, cancellationToken);
        IReadOnlyList<FlashcardSessionItem> items = await _flashcardSessionItemRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        return new FlashcardSessionResponse
        {
            SessionSqid = sessionSqid,
            StudentCourseSqid = session.StudentCourseId.HasValue ? _sqidService.Encode(session.StudentCourseId.Value) : null,
            DocumentSqid = TryResolveSessionDocumentSqid(items),
            ScopeType = session.ScopeType.ToString(),
            Status = session.Status.ToString(),
            CurrentItemIndex = session.CurrentItemIndex,
            StartedAt = session.StartedAt,
            LastActiveAt = session.LastActiveAt,
            Items = items.Select(ToItemResponse).ToList()
        };
    }

    public async Task<SubmitFlashcardSessionAnswerResponse> SubmitAnswerAsync(
        string sessionSqid,
        SubmitFlashcardSessionAnswerRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        long sessionId = DecodeRequiredSqid(sessionSqid, nameof(sessionSqid));
        long sessionItemId = DecodeRequiredSqid(request.SessionItemSqid, nameof(request.SessionItemSqid));

        FlashcardSession session = await GetTrackedSessionOrThrowAsync(sessionId, studentId, cancellationToken);
        FlashcardSessionItem sessionItem = await GetTrackedSessionItemOrThrowAsync(sessionItemId, sessionId, cancellationToken);
        IReadOnlyList<FlashcardSessionItem> sessionItems = await _flashcardSessionItemRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        StudentFlashcard progress = await GetTrackedProgressOrThrowAsync(sessionItem.StudentFlashcardId, studentId, cancellationToken);
        Flashcard flashcard = progress.Flashcard;

        FlashcardAnswerScoringResult scoringResult = await _flashcardAnswerScoringService.ScoreAsync(
            request.Answer,
            flashcard.Answer,
            request.ResponseTimeMs,
            cancellationToken);

        FlashcardAnswerHistory answerHistory = FlashcardAnswerHistory.Create(
            session.FlashcardSessionId,
            sessionItem.FlashcardSessionItemId,
            progress.StudentFlashcardId,
            request.Answer,
            flashcard.Answer,
            request.ResponseTimeMs,
            scoringResult.AiQualityScore,
            scoringResult.FallbackQualityScore,
            scoringResult.FinalQualityScore,
            scoringResult.WasAcceptedAsCorrect,
            scoringResult.ScoringSource,
            DateTime.UtcNow);

        progress.ApplyReviewQuality(scoringResult.FinalQualityScore, DateTime.UtcNow);
        StudentFlashcardAnalytics analytics = await _studentFlashcardAnalyticsService.RecomputeAsync(progress, cancellationToken);
        analytics.MarkAiPending(DateTime.UtcNow);

        bool showAgainInSession = scoringResult.FinalQualityScore <= 2;
        int? requeuedToOrder = null;
        if (showAgainInSession)
        {
            requeuedToOrder = session.CurrentItemIndex + 3;
            sessionItem.RequeueLater(requeuedToOrder.Value);
        }

        int nextItemIndex = session.CurrentItemIndex + 1;
        session.AdvanceTo(nextItemIndex);
        if (nextItemIndex >= sessionItems.Count)
        {
            session.MarkCompleted(DateTime.UtcNow);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _flashcardAnswerHistoryRepository.AddAsync(answerHistory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            sessionItem.MarkAnswered(answerHistory.FlashcardAnswerHistoryId, DateTime.UtcNow);
            await _studentFlashcardRepository.UpdateAsync(progress, cancellationToken);
            await _flashcardSessionItemRepository.UpdateAsync(sessionItem, cancellationToken);
            await _flashcardSessionRepository.UpdateAsync(session, cancellationToken);
            long studentCourseId = flashcard.Note.Document.Folder.StudentCourseId;
            await _performanceSummaryService.RefreshStudentCourseSummaryAsync(studentCourseId, cancellationToken);
            await _performanceSummaryService.RefreshOverallSummaryAsync(studentId, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await _studentFlashcardAnalyticsService.QueueAiEvaluationAsync(progress.StudentFlashcardId, cancellationToken);
        await _studentPerformanceAiWorkQueue.QueueAsync(
            new StudentPerformanceAiWorkItem(studentId, _sqidService.Encode(flashcard.Note.Document.Folder.StudentCourseId)),
            cancellationToken);

        return new SubmitFlashcardSessionAnswerResponse
        {
            SessionSqid = sessionSqid,
            SessionItemSqid = request.SessionItemSqid,
            QualityScore = scoringResult.FinalQualityScore,
            ShowAgainInSession = showAgainInSession,
            RequeuedToOrder = requeuedToOrder,
            NextReviewAt = progress.NextReviewAt,
            Progress = progress.ToProgressResponse(flashcard, _sqidService)
        };
    }

    public async Task<SubmitEvaluatedFlashcardSessionAnswerResponse> SubmitEvaluatedAnswerAsync(
        string sessionSqid,
        SubmitEvaluatedFlashcardSessionAnswerRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Evaluation);
        ArgumentNullException.ThrowIfNull(request.Analytics);

        long sessionId = DecodeRequiredSqid(sessionSqid, nameof(sessionSqid));
        long sessionItemId = DecodeRequiredSqid(request.SessionItemSqid, nameof(request.SessionItemSqid));
        DateTime now = DateTime.UtcNow;

        FlashcardSession session = await GetTrackedSessionOrThrowAsync(sessionId, studentId, cancellationToken);
        FlashcardSessionItem sessionItem = await GetTrackedSessionItemOrThrowAsync(sessionItemId, sessionId, cancellationToken);
        IReadOnlyList<FlashcardSessionItem> sessionItems = await _flashcardSessionItemRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        StudentFlashcard progress = await GetTrackedProgressOrThrowAsync(sessionItem.StudentFlashcardId, studentId, cancellationToken);
        Flashcard flashcard = progress.Flashcard;
        FlashcardAnswerVerdict verdict = ParseEnum<FlashcardAnswerVerdict>(request.Evaluation.Verdict, nameof(request.Evaluation.Verdict));
        string submittedAnswer = request.Answer?.Trim() ?? string.Empty;

        progress.ApplyEvaluation(
            request.Evaluation.AcceptedAsCorrect,
            request.Evaluation.QualityScore,
            verdict,
            now);

        await _studentFlashcardRepository.UpdateAsync(progress, cancellationToken);

        FlashcardAnswerHistory history = FlashcardAnswerHistory.Create(
            flashcardSessionId: session.FlashcardSessionId,
            flashcardSessionItemId: sessionItem.FlashcardSessionItemId,
            studentFlashcardId: progress.StudentFlashcardId,
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
            await _flashcardAnswerHistoryRepository.GetRecentByStudentFlashcardIdAsync(progress.StudentFlashcardId, 9, cancellationToken);

        List<FlashcardAnswerHistory> analyticsRecentAnswers = [history, .. existingRecentAnswers];
        await _studentFlashcardAnalyticsService.ApplyEvaluatedAttemptAnalyticsAsync(
            progress,
            request.Analytics,
            analyticsRecentAnswers,
            cancellationToken);

        bool showAgainInSession = request.Evaluation.QualityScore <= 2;
        int? requeuedToOrder = null;
        if (showAgainInSession)
        {
            requeuedToOrder = session.CurrentItemIndex + 3;
            sessionItem.RequeueLater(requeuedToOrder.Value);
        }

        int nextItemIndex = session.CurrentItemIndex + 1;
        session.AdvanceTo(nextItemIndex);
        if (nextItemIndex >= sessionItems.Count)
        {
            session.MarkCompleted(now);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            sessionItem.MarkAnswered(history.FlashcardAnswerHistoryId, now);
            await _flashcardSessionItemRepository.UpdateAsync(sessionItem, cancellationToken);
            await _flashcardSessionRepository.UpdateAsync(session, cancellationToken);

            long studentCourseId = flashcard.Note.Document.Folder.StudentCourseId;
            await _performanceSummaryService.RefreshStudentCourseSummaryAsync(studentCourseId, cancellationToken);
            await _performanceSummaryService.RefreshOverallSummaryAsync(studentId, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await _studentPerformanceAiWorkQueue.QueueAsync(
            new StudentPerformanceAiWorkItem(studentId, _sqidService.Encode(flashcard.Note.Document.Folder.StudentCourseId)),
            cancellationToken);

        StudentFlashcardAnalyticsResponse analytics =
            await _studentFlashcardAnalyticsService.GetByFlashcardSqidAsync(_sqidService.Encode(flashcard.FlashcardId), studentId, cancellationToken)
            ?? throw new StudentFlashcardValidationException("Unable to load flashcard analytics after the evaluated session answer was saved.");

        FlashcardSessionResponse refreshedSession = await ResumeSessionAsync(sessionSqid, studentId, cancellationToken);

        return new SubmitEvaluatedFlashcardSessionAnswerResponse
        {
            Session = refreshedSession,
            Answer = new FlashcardSessionAnswerEvaluationResultResponse
            {
                SessionItemSqid = request.SessionItemSqid,
                FlashcardSqid = _sqidService.Encode(flashcard.FlashcardId),
                QualityScore = request.Evaluation.QualityScore,
                ShowAgainInSession = showAgainInSession,
                RequeuedToOrder = requeuedToOrder,
                NextReviewAt = progress.NextReviewAt,
                Progress = progress.ToProgressResponse(flashcard, _sqidService),
                Evaluation = CreateEvaluationResponse(
                    verdict,
                    request.Evaluation.AcceptedAsCorrect,
                    request.Evaluation.QualityScore,
                    request.Evaluation.FeedbackSummary,
                    request.Evaluation.SemanticRationale),
                Analytics = analytics
            }
        };
    }

    public async Task<FlashcardSessionResponse> RestartSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long sessionId = DecodeRequiredSqid(sessionSqid, nameof(sessionSqid));
        FlashcardSession session = await GetTrackedSessionOrThrowAsync(sessionId, studentId, cancellationToken);
        IReadOnlyList<FlashcardSessionItem> existingItems = await _flashcardSessionItemRepository.GetBySessionIdAsync(session.FlashcardSessionId, cancellationToken);

        FlashcardSession restarted = session.RestartAsNew(DateTime.UtcNow);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _flashcardSessionRepository.UpdateAsync(session, cancellationToken);
            await _flashcardSessionRepository.AddAsync(restarted, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            List<FlashcardSessionItem> restartedItems = existingItems
                .Select(item => new FlashcardSessionItem(restarted.FlashcardSessionId, item.StudentFlashcardId, item.OriginalOrder))
                .ToList();

            await _flashcardSessionItemRepository.AddRangeAsync(restartedItems, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return await ResumeSessionAsync(_sqidService.Encode(restarted.FlashcardSessionId), studentId, cancellationToken);
    }

    public async Task AbandonSessionAsync(string sessionSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long sessionId = DecodeRequiredSqid(sessionSqid, nameof(sessionSqid));
        FlashcardSession session = await GetTrackedSessionOrThrowAsync(sessionId, studentId, cancellationToken);
        session.MarkAbandoned(DateTime.UtcNow);
        await _flashcardSessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private FlashcardSessionItemResponse ToItemResponse(FlashcardSessionItem item)
    {
        return new FlashcardSessionItemResponse
        {
            SessionItemSqid = _sqidService.Encode(item.FlashcardSessionItemId),
            FlashcardSqid = _sqidService.Encode(item.StudentFlashcard.FlashcardId),
            // Session items surface course context through the flashcard hierarchy instead of a duplicated StudentFlashcard FK.
            StudentCourseSqid = _sqidService.Encode(item.StudentFlashcard.Flashcard.Note.Document.Folder.StudentCourseId),
            Question = item.StudentFlashcard.Flashcard.Question,
            OriginalOrder = item.OriginalOrder,
            CurrentOrder = item.CurrentOrder,
            Status = item.Status.ToString()
        };
    }

    private async Task<FlashcardSession?> FindMatchingActiveSessionAsync(
        long studentId,
        long? studentCourseId,
        FlashcardSessionScopeType scopeType,
        long? documentId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FlashcardSession> sessions = await _flashcardSessionRepository.GetByStudentIdAsync(studentId, cancellationToken);

        IEnumerable<FlashcardSession> candidates = sessions
            .Where(session => session.ScopeType == scopeType)
            .Where(session => session.StudentCourseId == studentCourseId)
            .Where(session => session.Status == FlashcardSessionStatus.InProgress)
            .OrderByDescending(session => session.LastActiveAt);

        if (!documentId.HasValue)
        {
            return candidates.FirstOrDefault();
        }

        HashSet<long> expectedFlashcardIds = (await _flashcardRepository.GetAllByDocumentIdAndStudentIdAsync(
                documentId.Value,
                studentId,
                cancellationToken))
            .Select(flashcard => flashcard.FlashcardId)
            .ToHashSet();

        if (expectedFlashcardIds.Count == 0)
        {
            return null;
        }

          foreach (FlashcardSession candidate in candidates)
          {
              IReadOnlyList<FlashcardSessionItem> items = await _flashcardSessionItemRepository.GetBySessionIdAsync(
                  candidate.FlashcardSessionId,
                  cancellationToken);

              if (items.Count == 0)
              {
                  continue;
              }

              if (candidate.CurrentItemIndex >= items.Count)
              {
                  continue;
              }

              bool matchesDocument = items.All(item => item.StudentFlashcard.Flashcard.Note.DocumentId == documentId.Value);
              bool matchesFlashcardSet = items
                  .Select(item => item.StudentFlashcard.FlashcardId)
                .ToHashSet()
                .SetEquals(expectedFlashcardIds);

            if (matchesDocument && matchesFlashcardSet)
            {
                return candidate;
            }
        }

        return null;
    }

    private async Task<long?> ResolveStudentCourseIdAsync(
        FlashcardSessionScopeType scopeType,
        string? studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        if (scopeType == FlashcardSessionScopeType.Overall)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(studentCourseSqid))
        {
            throw new StudentCourseNotFoundException(nameof(studentCourseSqid));
        }

        long studentCourseId = DecodeRequiredSqid(studentCourseSqid, nameof(studentCourseSqid));
        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            throw new StudentCourseNotFoundException(studentCourseSqid);
        }

        return studentCourseId;
    }

    private async Task<long?> ResolveDocumentIdAsync(
        string? documentSqid,
        long studentId,
        long? studentCourseId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentSqid))
        {
            return null;
        }

        long documentId = DecodeRequiredSqid(documentSqid, nameof(documentSqid));
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null || document.Folder.StudentId != studentId)
        {
            throw new NotFoundException($"Document '{documentSqid}' was not found.");
        }

        if (studentCourseId.HasValue && document.Folder.StudentCourseId != studentCourseId.Value)
        {
            throw new NotFoundException($"Document '{documentSqid}' does not belong to the selected course.");
        }

        return documentId;
    }

    private async Task<IReadOnlyList<StudentFlashcardAnalytics>> GetDocumentCandidatesAsync(
        long studentId,
        long documentId,
        int take,
        CancellationToken cancellationToken)
    {
        List<Flashcard> flashcards = (await _flashcardRepository.GetAllByDocumentIdAndStudentIdAsync(
            documentId,
            studentId,
            cancellationToken))
            .Take(take)
            .ToList();

        if (flashcards.Count == 0)
        {
            return [];
        }

        long studentCourseId = flashcards[0].Note.Document.Folder.StudentCourseId;
        HashSet<long> flashcardIds = flashcards
            .Select(flashcard => flashcard.FlashcardId)
            .ToHashSet();

        Dictionary<long, StudentFlashcard> progressesByFlashcardId = (await _studentFlashcardRepository.GetAllByStudentCourseIdAsync(
                studentCourseId,
                cancellationToken))
            .Where(progress => progress.StudentId == studentId)
            .Where(progress => flashcardIds.Contains(progress.FlashcardId))
            .GroupBy(progress => progress.FlashcardId)
            .ToDictionary(group => group.Key, group => group.First());

        Dictionary<long, StudentFlashcardAnalytics> analyticsByStudentFlashcardId = (await _studentFlashcardAnalyticsRepository.GetByStudentCourseIdAsync(
                studentCourseId,
                cancellationToken))
            .Where(analytics => analytics.StudentFlashcard.StudentId == studentId)
            .Where(analytics => flashcardIds.Contains(analytics.StudentFlashcard.FlashcardId))
            .GroupBy(analytics => analytics.StudentFlashcardId)
            .ToDictionary(group => group.Key, group => group.First());

        List<StudentFlashcard> orderedProgresses = [];
        bool requiresProgressPersistence = false;

        foreach (Flashcard flashcard in flashcards)
        {
            if (!progressesByFlashcardId.TryGetValue(flashcard.FlashcardId, out StudentFlashcard? progress))
            {
                progress = await EnsureTrackedProgressForSessionAsync(studentId, flashcard, cancellationToken);
                progressesByFlashcardId[flashcard.FlashcardId] = progress;
            }

            if (progress.StudentFlashcardId <= 0)
            {
                requiresProgressPersistence = true;
            }

            orderedProgresses.Add(progress);
        }

        if (requiresProgressPersistence)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        List<StudentFlashcardAnalytics> candidates = [];

        foreach (StudentFlashcard progress in orderedProgresses)
        {
            if (!analyticsByStudentFlashcardId.TryGetValue(progress.StudentFlashcardId, out StudentFlashcardAnalytics? analytics))
            {
                analytics = new StudentFlashcardAnalytics(progress.StudentFlashcardId);
                analytics.InitializeDefaults(progress, DateTime.UtcNow);
                await _studentFlashcardAnalyticsRepository.AddAsync(analytics, cancellationToken);
                analyticsByStudentFlashcardId[progress.StudentFlashcardId] = analytics;
            }

            candidates.Add(analytics);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return candidates;
    }

    private async Task<StudentFlashcard> EnsureTrackedProgressForSessionAsync(
        long studentId,
        Flashcard flashcard,
        CancellationToken cancellationToken)
    {
        StudentFlashcard? active = await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(
            studentId,
            flashcard.FlashcardId,
            cancellationToken);

        if (active is not null)
        {
            return active;
        }

        DateTime now = DateTime.UtcNow;
        StudentFlashcard? archived = await _studentFlashcardRepository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(
            studentId,
            flashcard.FlashcardId,
            cancellationToken);

        if (archived is not null)
        {
            archived.Restore(now);
            archived.StartTracking(now);
            await _studentFlashcardRepository.UpdateAsync(archived, cancellationToken);
            return archived;
        }

        StudentFlashcard created = new(studentId, flashcard.FlashcardId);
        created.StartTracking(now);
        await _studentFlashcardRepository.AddAsync(created, cancellationToken);
        return created;
    }

    private string? TryResolveSessionDocumentSqid(IReadOnlyList<FlashcardSessionItem> items)
    {
        List<long> documentIds = items
            .Select(item => item.StudentFlashcard.Flashcard.Note.DocumentId)
            .Distinct()
            .Take(2)
            .ToList();

        if (documentIds.Count != 1)
        {
            return null;
        }

        return _sqidService.Encode(documentIds[0]);
    }

    private async Task<FlashcardSession> GetSessionOrThrowAsync(long sessionId, long studentId, CancellationToken cancellationToken)
    {
        FlashcardSession? session = await _flashcardSessionRepository.GetByIdAndStudentIdAsync(sessionId, studentId, cancellationToken);
        if (session is not null)
        {
            return session;
        }

        throw new NotFoundException($"Flashcard session '{sessionId}' was not found.");
    }

    private async Task<FlashcardSession> GetTrackedSessionOrThrowAsync(long sessionId, long studentId, CancellationToken cancellationToken)
    {
        FlashcardSession? session = await _flashcardSessionRepository.GetTrackedByIdAndStudentIdAsync(sessionId, studentId, cancellationToken);
        if (session is not null)
        {
            return session;
        }

        throw new NotFoundException($"Flashcard session '{sessionId}' was not found.");
    }

    private async Task<FlashcardSessionItem> GetTrackedSessionItemOrThrowAsync(long sessionItemId, long sessionId, CancellationToken cancellationToken)
    {
        FlashcardSessionItem? sessionItem = await _flashcardSessionItemRepository.GetTrackedByIdAsync(sessionItemId, cancellationToken);
        if (sessionItem is not null && sessionItem.FlashcardSessionId == sessionId)
        {
            return sessionItem;
        }

        throw new NotFoundException($"Flashcard session item '{sessionItemId}' was not found.");
    }

    private async Task<StudentFlashcard> GetTrackedProgressOrThrowAsync(long studentFlashcardId, long studentId, CancellationToken cancellationToken)
    {
        StudentFlashcard? progress = await _studentFlashcardRepository.GetTrackedByIdAndStudentIdAsync(studentFlashcardId, studentId, cancellationToken);
        if (progress is not null)
        {
            return progress;
        }

        throw new FlashcardNotFoundException(studentFlashcardId);
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long decodedId))
        {
            throw new InvalidSqidException(fieldName);
        }

        return decodedId;
    }

    private static int NormalizeBatchSize(int take)
    {
        if (take <= 0)
        {
            return 30;
        }

        return Math.Min(take, 100);
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
}
