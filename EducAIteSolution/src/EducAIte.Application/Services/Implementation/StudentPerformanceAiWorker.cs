using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Exceptions.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class StudentPerformanceAiWorker : BackgroundService
{
    private readonly IStudentPerformanceAiWorkQueue _workQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StudentPerformanceAiWorker> _logger;

    public StudentPerformanceAiWorker(
        IStudentPerformanceAiWorkQueue workQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<StudentPerformanceAiWorker> logger)
    {
        _workQueue = workQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (StudentPerformanceAiWorkItem workItem in _workQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(workItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Student performance AI refresh failed for student {StudentId} and student course {StudentCourseSqid}.",
                    workItem.StudentId,
                    workItem.StudentCourseSqid);
            }
        }
    }

    private async Task ProcessAsync(StudentPerformanceAiWorkItem workItem, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPerformanceSummaryService performanceSummaryService = scope.ServiceProvider.GetRequiredService<IPerformanceSummaryService>();
        IStudentPerformanceAiClient aiClient = scope.ServiceProvider.GetRequiredService<IStudentPerformanceAiClient>();

        await RefreshCourseSummaryAsync(workItem, performanceSummaryService, aiClient, cancellationToken);
        await RefreshOverallSummaryAsync(workItem.StudentId, performanceSummaryService, aiClient, cancellationToken);
    }

    private async Task RefreshCourseSummaryAsync(
        StudentPerformanceAiWorkItem workItem,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiClient aiClient,
        CancellationToken cancellationToken)
    {
        StudentCoursePerformanceSummaryEvaluationContextResponse? context =
            await performanceSummaryService.GetStudentCourseSummaryEvaluationContextAsync(
                workItem.StudentCourseSqid,
                workItem.StudentId,
                cancellationToken);

        if (context is null)
        {
            return;
        }

        await ApplyCourseSummaryAsync(
            workItem.StudentCourseSqid,
            workItem.StudentId,
            context,
            performanceSummaryService,
            aiClient,
            allowRetryOnConflict: true,
            cancellationToken);
    }

    private async Task RefreshOverallSummaryAsync(
        long studentId,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiClient aiClient,
        CancellationToken cancellationToken)
    {
        StudentOverallPerformanceSummaryEvaluationContextResponse? context =
            await performanceSummaryService.GetOverallSummaryEvaluationContextAsync(studentId, cancellationToken);

        if (context is null)
        {
            return;
        }

        await ApplyOverallSummaryAsync(
            studentId,
            context,
            performanceSummaryService,
            aiClient,
            allowRetryOnConflict: true,
            cancellationToken);
    }

    private async Task ApplyCourseSummaryAsync(
        string studentCourseSqid,
        long studentId,
        StudentCoursePerformanceSummaryEvaluationContextResponse context,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiClient aiClient,
        bool allowRetryOnConflict,
        CancellationToken cancellationToken)
    {
        GeneratedPerformanceSummaryAiResponse generatedSummary =
            await aiClient.EvaluateCourseSummaryAsync(context, cancellationToken);

        try
        {
            await performanceSummaryService.ApplyStudentCourseAiSummaryAsync(
                studentCourseSqid,
                new UpsertPerformanceSummaryAiRequest
                {
                    BasisLastComputedAt = context.Summary.LastComputedAt,
                    AiStatus = generatedSummary.AiStatus,
                    AiInsight = generatedSummary.AiInsight,
                    ImprovementSuggestion = generatedSummary.ImprovementSuggestion
                },
                studentId,
                cancellationToken);
        }
        catch (ConflictException) when (allowRetryOnConflict)
        {
            StudentCoursePerformanceSummaryEvaluationContextResponse? refreshedContext =
                await performanceSummaryService.GetStudentCourseSummaryEvaluationContextAsync(studentCourseSqid, studentId, cancellationToken);

            if (refreshedContext is null)
            {
                return;
            }

            await ApplyCourseSummaryAsync(
                studentCourseSqid,
                studentId,
                refreshedContext,
                performanceSummaryService,
                aiClient,
                allowRetryOnConflict: false,
                cancellationToken);
        }
    }

    private async Task ApplyOverallSummaryAsync(
        long studentId,
        StudentOverallPerformanceSummaryEvaluationContextResponse context,
        IPerformanceSummaryService performanceSummaryService,
        IStudentPerformanceAiClient aiClient,
        bool allowRetryOnConflict,
        CancellationToken cancellationToken)
    {
        GeneratedPerformanceSummaryAiResponse generatedSummary =
            await aiClient.EvaluateOverallSummaryAsync(context, cancellationToken);

        try
        {
            await performanceSummaryService.ApplyOverallAiSummaryAsync(
                new UpsertPerformanceSummaryAiRequest
                {
                    BasisLastComputedAt = context.Summary.LastComputedAt,
                    AiStatus = generatedSummary.AiStatus,
                    AiInsight = generatedSummary.AiInsight,
                    ImprovementSuggestion = generatedSummary.ImprovementSuggestion
                },
                studentId,
                cancellationToken);
        }
        catch (ConflictException) when (allowRetryOnConflict)
        {
            StudentOverallPerformanceSummaryEvaluationContextResponse? refreshedContext =
                await performanceSummaryService.GetOverallSummaryEvaluationContextAsync(studentId, cancellationToken);

            if (refreshedContext is null)
            {
                return;
            }

            await ApplyOverallSummaryAsync(
                studentId,
                refreshedContext,
                performanceSummaryService,
                aiClient,
                allowRetryOnConflict: false,
                cancellationToken);
        }
    }
}
