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
public class FlashcardWorkspaceController : ControllerBase
{
    private readonly IFlashcardWorkspaceService _flashcardWorkspaceService;

    public FlashcardWorkspaceController(IFlashcardWorkspaceService flashcardWorkspaceService)
    {
        _flashcardWorkspaceService = flashcardWorkspaceService;
    }

    [HttpGet("workspace/latest")]
    public async Task<IActionResult> GetLatestWorkspace(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        EducAIte.Application.DTOs.Response.FlashcardWorkspaceLatestResponse workspace =
            await _flashcardWorkspaceService.GetLatestWorkspaceAsync(studentId, cancellationToken);

        return Ok(workspace);
    }

    [HttpGet("student-courses/{studentCourseSqid}/documents")]
    public async Task<IActionResult> GetDocuments(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardDocumentResponse> documents =
            await _flashcardWorkspaceService.GetDocumentsAsync(studentCourseSqid, studentId, cancellationToken);

        return Ok(documents);
    }

    [HttpGet("documents/{documentSqid}/flashcards")]
    public async Task<IActionResult> GetFlashcards(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> flashcards =
            await _flashcardWorkspaceService.GetFlashcardsAsync(documentSqid, studentId, cancellationToken);

        return Ok(flashcards);
    }

    [HttpPost("documents/{documentSqid}/flashcards")]
    public async Task<IActionResult> CreateFlashcard(
        string documentSqid,
        [FromBody] CreateWorkspaceFlashcardRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardResponse created =
            await _flashcardWorkspaceService.CreateFlashcardAsync(documentSqid, request, studentId, cancellationToken);

        return Ok(created);
    }

    [HttpPut("documents/{documentSqid}/flashcards/{flashcardSqid}")]
    public async Task<IActionResult> UpdateFlashcard(
        string documentSqid,
        string flashcardSqid,
        [FromBody] UpdateWorkspaceFlashcardRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool updated = await _flashcardWorkspaceService.UpdateFlashcardAsync(
            documentSqid,
            flashcardSqid,
            request,
            studentId,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("documents/{documentSqid}/flashcards/{flashcardSqid}")]
    public async Task<IActionResult> DeleteFlashcard(
        string documentSqid,
        string flashcardSqid,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool deleted = await _flashcardWorkspaceService.DeleteFlashcardAsync(
            documentSqid,
            flashcardSqid,
            studentId,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
