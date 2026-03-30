using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FlashcardSessionController : ControllerBase
{
    private readonly IFlashcardSessionService _flashcardSessionService;

    public FlashcardSessionController(IFlashcardSessionService flashcardSessionService)
    {
        _flashcardSessionService = flashcardSessionService;
    }

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartFlashcardSessionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardSessionResponse session = await _flashcardSessionService.StartSessionAsync(request, studentId, cancellationToken);
        return Ok(session);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(
        [FromQuery] FlashcardSessionScopeType scopeType,
        [FromQuery] string? studentCourseSqid,
        [FromQuery] string? documentSqid,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardSessionResponse? session =
            await _flashcardSessionService.GetActiveSessionAsync(scopeType, studentCourseSqid, documentSqid, studentId, cancellationToken);

        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost("{sessionSqid}/resume")]
    public async Task<IActionResult> Resume(string sessionSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardSessionResponse session = await _flashcardSessionService.ResumeSessionAsync(sessionSqid, studentId, cancellationToken);
        return Ok(session);
    }

    [HttpPost("{sessionSqid}/answers")]
    public async Task<IActionResult> SubmitAnswer(
        string sessionSqid,
        [FromBody] SubmitFlashcardSessionAnswerRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        SubmitFlashcardSessionAnswerResponse response =
            await _flashcardSessionService.SubmitAnswerAsync(sessionSqid, request, studentId, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{sessionSqid}/evaluated-answers")]
    public async Task<IActionResult> SubmitEvaluatedAnswer(
        string sessionSqid,
        [FromBody] SubmitEvaluatedFlashcardSessionAnswerRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        SubmitEvaluatedFlashcardSessionAnswerResponse response =
            await _flashcardSessionService.SubmitEvaluatedAnswerAsync(sessionSqid, request, studentId, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{sessionSqid}/restart")]
    public async Task<IActionResult> Restart(string sessionSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardSessionResponse session = await _flashcardSessionService.RestartSessionAsync(sessionSqid, studentId, cancellationToken);
        return Ok(session);
    }

    [HttpPost("{sessionSqid}/abandon")]
    public async Task<IActionResult> Abandon(string sessionSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        await _flashcardSessionService.AbandonSessionAsync(sessionSqid, studentId, cancellationToken);
        return NoContent();
    }

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
