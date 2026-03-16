using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly INoteOrderingService _noteOrderingService;

    public NoteController(INoteService noteService, INoteOrderingService noteOrderingService)
    {
        _noteService = noteService;
        _noteOrderingService = noteOrderingService;
    }

    [HttpGet("{sqid}")]
    public async Task<IActionResult> GetById(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        NoteResponse? note;
        try
        {
            note = await _noteService.GetNoteBySqidAsync(sqid, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (note is null)
        {
            return NotFound();
        }

        return Ok(note);
    }

    [HttpGet("document/{documentSqid}")]
    public async Task<IActionResult> GetByDocument(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IEnumerable<NoteResponse> notes;
        try
        {
            notes = await _noteService.GetNotesByDocumentAsync(documentSqid, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        return Ok(notes);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<NoteResponse> notes = await _noteService.GetNotesByStudentAsync(studentId, cancellationToken);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        NoteResponse createdNote;
        try
        {
            createdNote = await _noteService.CreateNoteAsync(request, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        return CreatedAtAction(nameof(GetById), new { sqid = createdNote.Sqid }, createdNote);
    }

    [HttpPost("/api/documents/{documentSqid}/notes/{noteSqid}/move")]
    public async Task<IActionResult> Move(
        string documentSqid,
        string noteSqid,
        [FromBody] MoveNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            bool moved = await _noteOrderingService.MoveBetweenAsync(
                studentId,
                documentSqid,
                noteSqid,
                request.PreviousNoteSqid,
                request.NextNoteSqid,
                cancellationToken);

            if (!moved)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("/api/documents/{documentSqid}/notes/rebalance")]
    public async Task<IActionResult> Rebalance(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            await _noteOrderingService.RebalanceAsync(studentId, documentSqid, cancellationToken);
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

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isUpdated;
        try
        {
            isUpdated = await _noteService.UpdateNoteAsync(sqid, request, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (!isUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{sqid}")]
    public async Task<IActionResult> Patch(string sqid, [FromBody] PatchNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isPatched;
        try
        {
            isPatched = await _noteService.PatchNoteAsync(sqid, request, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (!isPatched)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{sqid}")]
    public async Task<IActionResult> Delete(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isDeleted;
        try
        {
            isDeleted = await _noteService.DeleteNoteAsync(sqid, studentId, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (!isDeleted)
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
