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

        NoteResponse? note = await _noteService.GetNoteBySqidAsync(sqid, studentId, cancellationToken);

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

        IEnumerable<NoteResponse> notes = await _noteService.GetNotesByDocumentAsync(documentSqid, studentId, cancellationToken);

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

        NoteResponse createdNote = await _noteService.CreateNoteAsync(request, studentId, cancellationToken);

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

    [HttpPost("/api/documents/{documentSqid}/notes/rebalance")]
    public async Task<IActionResult> Rebalance(string documentSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        await _noteOrderingService.RebalanceAsync(studentId, documentSqid, cancellationToken);
        return NoContent();
    }

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isUpdated = await _noteService.UpdateNoteAsync(sqid, request, studentId, cancellationToken);

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

        bool isPatched = await _noteService.PatchNoteAsync(sqid, request, studentId, cancellationToken);

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

        bool isDeleted = await _noteService.DeleteNoteAsync(sqid, studentId, cancellationToken);

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
