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
public sealed class FlashcardAnalyticsController : ControllerBase
{
    private readonly IStudentFlashcardAnalyticsService _studentFlashcardAnalyticsService;

    public FlashcardAnalyticsController(IStudentFlashcardAnalyticsService studentFlashcardAnalyticsService)
    {
        _studentFlashcardAnalyticsService = studentFlashcardAnalyticsService;
    }

    [HttpGet("{flashcardSqid}")]
    public async Task<IActionResult> GetByFlashcard(string flashcardSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardAnalyticsResponse? analytics =
            await _studentFlashcardAnalyticsService.GetByFlashcardSqidAsync(flashcardSqid, studentId, cancellationToken);

        return analytics is null ? NotFound() : Ok(analytics);
    }

    [HttpGet("{flashcardSqid}/evaluation-context")]
    public async Task<IActionResult> GetEvaluationContext(string flashcardSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardAnalyticsEvaluationContextResponse? context =
            await _studentFlashcardAnalyticsService.GetEvaluationContextAsync(flashcardSqid, studentId, cancellationToken);

        return context is null ? NotFound() : Ok(context);
    }

    [HttpPut("{flashcardSqid}/ai-evaluation")]
    public async Task<IActionResult> ApplyAiEvaluation(
        string flashcardSqid,
        [FromBody] UpsertFlashcardAnalyticsAiRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        StudentFlashcardAnalyticsResponse updatedAnalytics =
            await _studentFlashcardAnalyticsService.ApplyAiEvaluationAsync(flashcardSqid, request, studentId, cancellationToken);

        return Ok(updatedAnalytics);
    }

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
