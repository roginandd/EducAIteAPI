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
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;

    public FlashcardController(IFlashcardService flashcardService)
    {
        _flashcardService = flashcardService;
    }

    [HttpGet("{sqid}")]
    public async Task<IActionResult> GetById(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FlashcardResponse? flashcard;
        try
        {
            flashcard = await _flashcardService.GetBySqidAsync(sqid, studentId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (flashcard is null)
        {
            return NotFound();
        }

        return Ok(flashcard);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FlashcardResponse> flashcards = await _flashcardService.GetMineAsync(studentId, cancellationToken);
        return Ok(flashcards);
    }

    [HttpGet("document/{documentSqid}")]
    public async Task<IActionResult> GetByDocument(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<FlashcardResponse> flashcards = await _flashcardService.GetByDocumentAsync(documentSqid, studentId, cancellationToken);
            return Ok(flashcards);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlashcardRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            FlashcardResponse created = await _flashcardService.CreateAsync(request, studentId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { sqid = created.Sqid }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateFlashcardRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            bool updated = await _flashcardService.UpdateAsync(sqid, request, studentId, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("{sqid}")]
    public async Task<IActionResult> Delete(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool deleted;
        try
        {
            deleted = await _flashcardService.DeleteAsync(sqid, studentId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

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
