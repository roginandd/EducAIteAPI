using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.StudentCourse;
using EducAIte.Domain.Interfaces;

namespace EducAIte.Application.Services.Implementation;

public sealed class PerformanceSummaryService : IPerformanceSummaryService
{
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IStudentFlashcardAnalyticsRepository _studentFlashcardAnalyticsRepository;
    private readonly IFlashcardSessionRepository _flashcardSessionRepository;
    private readonly IStudentCoursePerformanceSummaryRepository _studentCoursePerformanceSummaryRepository;
    private readonly IStudentOverallPerformanceSummaryRepository _studentOverallPerformanceSummaryRepository;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;

    public PerformanceSummaryService(
        IStudentCourseRepository studentCourseRepository,
        IStudentFlashcardRepository studentFlashcardRepository,
        IStudentFlashcardAnalyticsRepository studentFlashcardAnalyticsRepository,
        IFlashcardSessionRepository flashcardSessionRepository,
        IStudentCoursePerformanceSummaryRepository studentCoursePerformanceSummaryRepository,
        IStudentOverallPerformanceSummaryRepository studentOverallPerformanceSummaryRepository,
        ISqidService sqidService,
        IUnitOfWork unitOfWork)
    {
        _studentCourseRepository = studentCourseRepository;
        _studentFlashcardRepository = studentFlashcardRepository;
        _studentFlashcardAnalyticsRepository = studentFlashcardAnalyticsRepository;
        _flashcardSessionRepository = flashcardSessionRepository;
        _studentCoursePerformanceSummaryRepository = studentCoursePerformanceSummaryRepository;
        _studentOverallPerformanceSummaryRepository = studentOverallPerformanceSummaryRepository;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
    }

    public async Task RefreshStudentCourseSummaryAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StudentFlashcard> progressRows = await _studentFlashcardRepository.GetAllByStudentCourseIdAsync(studentCourseId, cancellationToken);
        IReadOnlyList<StudentFlashcardAnalytics> analyticsRows = await _studentFlashcardAnalyticsRepository.GetByStudentCourseIdAsync(studentCourseId, cancellationToken);

        StudentCoursePerformanceSummary summary =
            await _studentCoursePerformanceSummaryRepository.GetTrackedByStudentCourseIdAsync(studentCourseId, cancellationToken)
            ?? new StudentCoursePerformanceSummary(studentCourseId);

        summary.Recompute(progressRows, analyticsRows, DateTime.UtcNow);

        if (await _studentCoursePerformanceSummaryRepository.GetTrackedByStudentCourseIdAsync(studentCourseId, cancellationToken) is null)
        {
            await _studentCoursePerformanceSummaryRepository.AddAsync(summary, cancellationToken);
        }
        else
        {
            await _studentCoursePerformanceSummaryRepository.UpdateAsync(summary, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RefreshOverallSummaryAsync(long studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StudentCoursePerformanceSummary> courseSummaries =
            await _studentCoursePerformanceSummaryRepository.GetByStudentIdAsync(studentId, cancellationToken);

        StudentOverallPerformanceSummary summary =
            await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken)
            ?? new StudentOverallPerformanceSummary(studentId);

        summary.Recompute(courseSummaries, DateTime.UtcNow);

        if (await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken) is null)
        {
            await _studentOverallPerformanceSummaryRepository.AddAsync(summary, cancellationToken);
        }
        else
        {
            await _studentOverallPerformanceSummaryRepository.UpdateAsync(summary, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<StudentAnalyticsDashboardResponse> GetDashboardAsync(long studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StudentCourse> studentCourses =
            await _studentCourseRepository.GetAllByStudentIdAsync(studentId, cancellationToken: cancellationToken);

        List<StudentCourse> orderedCourses = studentCourses
            .OrderBy(studentCourse => studentCourse.CreatedAt)
            .ToList();

        IReadOnlyList<StudentCoursePerformanceSummary> courseSummaries =
            await _studentCoursePerformanceSummaryRepository.GetByStudentIdAsync(studentId, cancellationToken);

        StudentOverallPerformanceSummary? overallSummary =
            await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken);

        IReadOnlyList<FlashcardSession> flashcardSessions =
            await _flashcardSessionRepository.GetByStudentIdAsync(studentId, cancellationToken);

        Dictionary<long, StudentCoursePerformanceSummary> courseSummaryById = courseSummaries
            .GroupBy(summary => summary.StudentCourseId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(summary => summary.LastComputedAt).First());

        Dictionary<long, decimal> studyTimeByCourseId = CalculateStudyTimeHoursByCourseId(flashcardSessions);

        return new StudentAnalyticsDashboardResponse
        {
            OverallPerformance = ToOverallPerformanceDto(overallSummary),
            LearningTrendAnalysis = new LearningTrendAnalysisDTO
            {
                Items = orderedCourses
                    .Select(studentCourse => ToCourseStudyTimeItem(studentCourse, studyTimeByCourseId))
                    .OrderByDescending(item => item.StudyTimeHours)
                    .ToList()
            },
            BestPerformingCourse = new BestPerformingCourseDTO
            {
                Items = orderedCourses
                    .Select(studentCourse => ToOverallPerformanceItem(studentCourse, courseSummaryById))
                    .OrderByDescending(item => item.OverallPerformanceScore)
                    .ToList()
            },
            LearningRetentionRate = new LearningRetentionRateDTO
            {
                Items = orderedCourses
                    .Select(studentCourse => ToLearningRetentionItem(studentCourse, courseSummaryById))
                    .OrderByDescending(item => item.LearningRetentionRate)
                    .ToList()
            },
            PerformanceSummaryRate = new PerformanceSummaryRateDTO
            {
                Items = orderedCourses
                    .Select(studentCourse => ToPerformanceSummaryItem(studentCourse, courseSummaryById))
                    .ToList()
            }
        };
    }

    public async Task<StudentCoursePerformanceSummaryEvaluationContextResponse?> GetStudentCourseSummaryEvaluationContextAsync(
        string studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        long studentCourseId = DecodeRequiredSqid(studentCourseSqid, nameof(studentCourseSqid));
        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            return null;
        }

        StudentCoursePerformanceSummary? summary =
            await _studentCoursePerformanceSummaryRepository.GetTrackedByStudentCourseIdAsync(studentCourseId, cancellationToken);

        if (summary is null)
        {
            return null;
        }

        IReadOnlyList<StudentFlashcardAnalytics> analyticsRows =
            await _studentFlashcardAnalyticsRepository.GetByStudentCourseIdAsync(studentCourseId, cancellationToken);

        IReadOnlyList<CoursePerformanceSummaryFlashcardContextResponse> topRiskFlashcards = analyticsRows
            .OrderByDescending(analytics => analytics.RiskLevel)
            .ThenBy(analytics => analytics.ConfidenceScore)
            .ThenBy(analytics => analytics.RetentionScore)
            .Take(5)
            .Select(analytics => new CoursePerformanceSummaryFlashcardContextResponse
            {
                FlashcardSqid = _sqidService.Encode(analytics.StudentFlashcard.FlashcardId),
                Question = analytics.StudentFlashcard.Flashcard.Question,
                MasteryLevel = analytics.MasteryLevel.ToString(),
                ConfidenceScore = analytics.ConfidenceScore,
                RetentionScore = analytics.RetentionScore,
                RiskLevel = analytics.RiskLevel.ToString(),
                AiInsight = analytics.AiInsight
            })
            .ToList();

        return new StudentCoursePerformanceSummaryEvaluationContextResponse
        {
            StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
            CourseName = studentCourse.Course.CourseName,
            EdpCode = studentCourse.Course.EDPCode,
            Summary = ToCourseSummarySnapshot(summary),
            TopRiskFlashcards = topRiskFlashcards
        };
    }

    public async Task<StudentOverallPerformanceSummaryEvaluationContextResponse?> GetOverallSummaryEvaluationContextAsync(
        long studentId,
        CancellationToken cancellationToken = default)
    {
        StudentOverallPerformanceSummary? summary =
            await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken);

        if (summary is null)
        {
            return null;
        }

        IReadOnlyList<StudentCoursePerformanceSummary> courseSummaries =
            await _studentCoursePerformanceSummaryRepository.GetByStudentIdAsync(studentId, cancellationToken);

        IReadOnlyList<CoursePerformanceSummaryBreakdownResponse> courseBreakdown = courseSummaries
            .OrderByDescending(courseSummary => courseSummary.RiskLevel)
            .ThenBy(courseSummary => courseSummary.OverallPerformanceScore)
            .Select(courseSummary => new CoursePerformanceSummaryBreakdownResponse
            {
                StudentCourseSqid = _sqidService.Encode(courseSummary.StudentCourseId),
                CourseName = courseSummary.StudentCourse.Course.CourseName,
                EdpCode = courseSummary.StudentCourse.Course.EDPCode,
                TrackedFlashcardsCount = courseSummary.TrackedFlashcardsCount,
                MasteredFlashcardsCount = courseSummary.MasteredFlashcardsCount,
                FlashcardAccuracyRate = courseSummary.FlashcardAccuracyRate,
                LearningRetentionRate = courseSummary.LearningRetentionRate,
                ConfidenceScore = courseSummary.ConfidenceScore,
                OverallPerformanceScore = courseSummary.OverallPerformanceScore,
                RiskLevel = courseSummary.RiskLevel.ToString(),
                AiInsight = courseSummary.AiInsight
            })
            .ToList();

        return new StudentOverallPerformanceSummaryEvaluationContextResponse
        {
            Summary = ToOverallSummarySnapshot(summary),
            CourseBreakdown = courseBreakdown
        };
    }

    public async Task<StudentCoursePerformanceSummaryResponse> ApplyStudentCourseAiSummaryAsync(
        string studentCourseSqid,
        UpsertPerformanceSummaryAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        long studentCourseId = DecodeRequiredSqid(studentCourseSqid, nameof(studentCourseSqid));
        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            throw new StudentCourseNotFoundException(studentCourseSqid);
        }

        StudentCoursePerformanceSummary summary =
            await _studentCoursePerformanceSummaryRepository.GetTrackedByStudentCourseIdAsync(studentCourseId, cancellationToken)
            ?? throw new NotFoundException($"Student course performance summary for '{studentCourseSqid}' was not found.");

        EnsureSummaryContextIsCurrent(summary.LastComputedAt, request.BasisLastComputedAt);

        summary.ApplyAiSummary(
            request.AiInsight,
            request.ImprovementSuggestion,
            ParseAiStatus(request.AiStatus),
            DateTime.UtcNow);

        await _studentCoursePerformanceSummaryRepository.UpdateAsync(summary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(summary);
    }

    public async Task<StudentOverallPerformanceSummaryResponse> ApplyOverallAiSummaryAsync(
        UpsertPerformanceSummaryAiRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        StudentOverallPerformanceSummary summary =
            await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("Student overall performance summary was not found.");

        EnsureSummaryContextIsCurrent(summary.LastComputedAt, request.BasisLastComputedAt);

        summary.ApplyAiSummary(
            request.AiInsight,
            request.ImprovementSuggestion,
            ParseAiStatus(request.AiStatus),
            DateTime.UtcNow);

        await _studentOverallPerformanceSummaryRepository.UpdateAsync(summary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(summary);
    }

    public async Task<StudentCoursePerformanceSummaryResponse?> GetStudentCourseSummaryAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default)
    {
        long studentCourseId = DecodeRequiredSqid(studentCourseSqid, nameof(studentCourseSqid));
        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            return null;
        }

        StudentCoursePerformanceSummary? summary =
            await _studentCoursePerformanceSummaryRepository.GetTrackedByStudentCourseIdAsync(studentCourseId, cancellationToken);

        return summary is null ? null : ToResponse(summary);
    }

    public async Task<StudentOverallPerformanceSummaryResponse?> GetOverallSummaryAsync(long studentId, CancellationToken cancellationToken = default)
    {
        StudentOverallPerformanceSummary? summary =
            await _studentOverallPerformanceSummaryRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken);

        return summary is null ? null : ToResponse(summary);
    }

    private StudentCoursePerformanceSummaryResponse ToResponse(StudentCoursePerformanceSummary summary)
    {
        return new StudentCoursePerformanceSummaryResponse
        {
            StudentCourseSqid = _sqidService.Encode(summary.StudentCourseId),
            TrackedFlashcardsCount = summary.TrackedFlashcardsCount,
            MasteredFlashcardsCount = summary.MasteredFlashcardsCount,
            FlashcardAccuracyRate = summary.FlashcardAccuracyRate,
            LearningRetentionRate = summary.LearningRetentionRate,
            ConfidenceScore = summary.ConfidenceScore,
            OverallPerformanceScore = summary.OverallPerformanceScore,
            RiskLevel = summary.RiskLevel.ToString(),
            AiStatus = summary.AiStatus.ToString(),
            AiInsight = summary.AiInsight,
            ImprovementSuggestion = summary.ImprovementSuggestion,
            LastComputedAt = summary.LastComputedAt
        };
    }

    private static StudentOverallPerformanceSummaryResponse ToResponse(StudentOverallPerformanceSummary summary)
    {
        return new StudentOverallPerformanceSummaryResponse
        {
            TrackedCoursesCount = summary.TrackedCoursesCount,
            TrackedFlashcardsCount = summary.TrackedFlashcardsCount,
            MasteredFlashcardsCount = summary.MasteredFlashcardsCount,
            FlashcardAccuracyRate = summary.FlashcardAccuracyRate,
            LearningRetentionRate = summary.LearningRetentionRate,
            ConfidenceScore = summary.ConfidenceScore,
            OverallPerformanceScore = summary.OverallPerformanceScore,
            RiskLevel = summary.RiskLevel.ToString(),
            AiStatus = summary.AiStatus.ToString(),
            AiInsight = summary.AiInsight,
            ImprovementSuggestion = summary.ImprovementSuggestion,
            LastComputedAt = summary.LastComputedAt
        };
    }

    private OverallPerformanceDTO ToOverallPerformanceDto(StudentOverallPerformanceSummary? summary)
    {
        if (summary is null)
        {
            return new OverallPerformanceDTO
            {
                AiStatus = "InsufficientSignal",
                RiskLevel = "High"
            };
        }

        return new OverallPerformanceDTO
        {
            TrackedCoursesCount = summary.TrackedCoursesCount,
            TrackedFlashcardsCount = summary.TrackedFlashcardsCount,
            MasteredFlashcardsCount = summary.MasteredFlashcardsCount,
            FlashcardAccuracyRate = summary.FlashcardAccuracyRate,
            LearningRetentionRate = summary.LearningRetentionRate,
            ConfidenceScore = summary.ConfidenceScore,
            OverallPerformanceScore = summary.OverallPerformanceScore,
            RiskLevel = summary.RiskLevel.ToString(),
            AiStatus = summary.AiStatus.ToString(),
            AiInsight = summary.AiInsight,
            ImprovementSuggestion = summary.ImprovementSuggestion,
            LastComputedAt = summary.LastComputedAt
        };
    }

    private CourseStudyTimeItemDTO ToCourseStudyTimeItem(StudentCourse studentCourse, IReadOnlyDictionary<long, decimal> studyTimeByCourseId)
    {
        studyTimeByCourseId.TryGetValue(studentCourse.StudentCourseId, out decimal studyTimeHours);

        return new CourseStudyTimeItemDTO
        {
            StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
            CourseName = studentCourse.Course.CourseName,
            EdpCode = studentCourse.Course.EDPCode,
            StudyTimeHours = studyTimeHours
        };
    }

    private CourseOverallPerformanceItemDTO ToOverallPerformanceItem(
        StudentCourse studentCourse,
        IReadOnlyDictionary<long, StudentCoursePerformanceSummary> courseSummaryById)
    {
        decimal overallPerformanceScore = courseSummaryById.TryGetValue(studentCourse.StudentCourseId, out StudentCoursePerformanceSummary? summary)
            ? summary.OverallPerformanceScore
            : 0m;

        return new CourseOverallPerformanceItemDTO
        {
            StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
            CourseName = studentCourse.Course.CourseName,
            EdpCode = studentCourse.Course.EDPCode,
            OverallPerformanceScore = overallPerformanceScore
        };
    }

    private CourseLearningRetentionItemDTO ToLearningRetentionItem(
        StudentCourse studentCourse,
        IReadOnlyDictionary<long, StudentCoursePerformanceSummary> courseSummaryById)
    {
        decimal learningRetentionRate = courseSummaryById.TryGetValue(studentCourse.StudentCourseId, out StudentCoursePerformanceSummary? summary)
            ? summary.LearningRetentionRate
            : 0m;

        return new CourseLearningRetentionItemDTO
        {
            StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
            CourseName = studentCourse.Course.CourseName,
            EdpCode = studentCourse.Course.EDPCode,
            LearningRetentionRate = learningRetentionRate
        };
    }

    private CoursePerformanceSummaryItemDTO ToPerformanceSummaryItem(
        StudentCourse studentCourse,
        IReadOnlyDictionary<long, StudentCoursePerformanceSummary> courseSummaryById)
    {
        decimal overallPerformanceScore = courseSummaryById.TryGetValue(studentCourse.StudentCourseId, out StudentCoursePerformanceSummary? summary)
            ? summary.OverallPerformanceScore
            : 0m;

        return new CoursePerformanceSummaryItemDTO
        {
            StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
            CourseName = studentCourse.Course.CourseName,
            EdpCode = studentCourse.Course.EDPCode,
            OverallPerformanceScore = overallPerformanceScore
        };
    }

    private static Dictionary<long, decimal> CalculateStudyTimeHoursByCourseId(IReadOnlyList<FlashcardSession> sessions)
    {
        return sessions
            .Where(session => session.StudentCourseId.HasValue)
            // The dashboard trend uses elapsed study time per course based on the recorded session window.
            .Select(session => new
            {
                StudentCourseId = session.StudentCourseId!.Value,
                Duration = GetSessionDuration(session)
            })
            .GroupBy(item => item.StudentCourseId)
            .ToDictionary(
                group => group.Key,
                group => decimal.Round((decimal)group.Sum(item => item.Duration.TotalHours), 2));
    }

    private static TimeSpan GetSessionDuration(FlashcardSession session)
    {
        DateTime endTime = session.CompletedAt?.ToUniversalTime() ?? session.LastActiveAt.ToUniversalTime();
        DateTime startTime = session.StartedAt.ToUniversalTime();

        if (endTime <= startTime)
        {
            return TimeSpan.Zero;
        }

        return endTime - startTime;
    }

    private static CoursePerformanceSummarySnapshotResponse ToCourseSummarySnapshot(StudentCoursePerformanceSummary summary)
    {
        return new CoursePerformanceSummarySnapshotResponse
        {
            TrackedFlashcardsCount = summary.TrackedFlashcardsCount,
            MasteredFlashcardsCount = summary.MasteredFlashcardsCount,
            FlashcardAccuracyRate = summary.FlashcardAccuracyRate,
            LearningRetentionRate = summary.LearningRetentionRate,
            ConfidenceScore = summary.ConfidenceScore,
            OverallPerformanceScore = summary.OverallPerformanceScore,
            RiskLevel = summary.RiskLevel.ToString(),
            AiStatus = summary.AiStatus.ToString(),
            AiInsight = summary.AiInsight,
            ImprovementSuggestion = summary.ImprovementSuggestion,
            LastComputedAt = summary.LastComputedAt
        };
    }

    private static OverallPerformanceSummarySnapshotResponse ToOverallSummarySnapshot(StudentOverallPerformanceSummary summary)
    {
        return new OverallPerformanceSummarySnapshotResponse
        {
            TrackedCoursesCount = summary.TrackedCoursesCount,
            TrackedFlashcardsCount = summary.TrackedFlashcardsCount,
            MasteredFlashcardsCount = summary.MasteredFlashcardsCount,
            FlashcardAccuracyRate = summary.FlashcardAccuracyRate,
            LearningRetentionRate = summary.LearningRetentionRate,
            ConfidenceScore = summary.ConfidenceScore,
            OverallPerformanceScore = summary.OverallPerformanceScore,
            RiskLevel = summary.RiskLevel.ToString(),
            AiStatus = summary.AiStatus.ToString(),
            AiInsight = summary.AiInsight,
            ImprovementSuggestion = summary.ImprovementSuggestion,
            LastComputedAt = summary.LastComputedAt
        };
    }

    private static void EnsureSummaryContextIsCurrent(DateTime currentLastComputedAt, DateTime requestedBasisLastComputedAt)
    {
        if (currentLastComputedAt.ToUniversalTime() != requestedBasisLastComputedAt.ToUniversalTime())
        {
            throw new ConflictException(
                "The performance summary changed since the AI evaluation context was fetched.",
                "summary_context_stale");
        }
    }

    private static AiEvaluationStatus ParseAiStatus(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue) || !Enum.TryParse(rawValue.Trim(), true, out AiEvaluationStatus parsedStatus))
        {
            throw new ValidationException("aiStatus is invalid.", "invalid_ai_status");
        }

        return parsedStatus;
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long decodedId))
        {
            throw new InvalidSqidException(fieldName);
        }

        return decodedId;
    }
}
