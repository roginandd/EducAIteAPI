using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StudentPerformanceController : ControllerBase
{
    private readonly IPerformanceSummaryService _performanceSummaryService;

    public StudentPerformanceController(IPerformanceSummaryService performanceSummaryService)
    {
        _performanceSummaryService = performanceSummaryService;
    }

    [HttpGet("student-courses/{studentCourseSqid}")]
    public async Task<IActionResult> GetStudentCourseSummary(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentCoursePerformanceSummaryResponse? summary =
            await _performanceSummaryService.GetStudentCourseSummaryAsync(studentCourseSqid, studentId, cancellationToken);

        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpGet("student-courses/{studentCourseSqid}/evaluation-context")]
    public async Task<IActionResult> GetStudentCourseSummaryEvaluationContext(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentCoursePerformanceSummaryEvaluationContextResponse? context =
            await _performanceSummaryService.GetStudentCourseSummaryEvaluationContextAsync(studentCourseSqid, studentId, cancellationToken);

        return context is null ? NotFound() : Ok(context);
    }

    [HttpPut("student-courses/{studentCourseSqid}/ai-summary")]
    public async Task<IActionResult> ApplyStudentCourseAiSummary(
        string studentCourseSqid,
        [FromBody] UpsertPerformanceSummaryAiRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentCoursePerformanceSummaryResponse updatedSummary =
            await _performanceSummaryService.ApplyStudentCourseAiSummaryAsync(studentCourseSqid, request, studentId, cancellationToken);

        return Ok(updatedSummary);
    }

    [HttpGet("overall")]
    public async Task<IActionResult> GetOverall(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentOverallPerformanceSummaryResponse? summary =
            await _performanceSummaryService.GetOverallSummaryAsync(studentId, cancellationToken);

        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpGet("overall/evaluation-context")]
    public async Task<IActionResult> GetOverallEvaluationContext(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentOverallPerformanceSummaryEvaluationContextResponse? context =
            await _performanceSummaryService.GetOverallSummaryEvaluationContextAsync(studentId, cancellationToken);

        return context is null ? NotFound() : Ok(context);
    }

    [HttpPut("overall/ai-summary")]
    public async Task<IActionResult> ApplyOverallAiSummary(
        [FromBody] UpsertPerformanceSummaryAiRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentOverallPerformanceSummaryResponse updatedSummary =
            await _performanceSummaryService.ApplyOverallAiSummaryAsync(request, studentId, cancellationToken);

        return Ok(updatedSummary);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentAnalyticsDashboardResponse dashboard =
            await _performanceSummaryService.GetDashboardAsync(studentId, cancellationToken);

        return Ok(dashboard);
    }

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
