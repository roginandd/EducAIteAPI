using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet("{externalId:guid}")]
    public async Task<IActionResult> GetById(Guid externalId, CancellationToken cancellationToken)
    {
        NoteResponse? note = await _noteService.GetNoteByExternalIdAsync(externalId, cancellationToken);
        if (note is null)
        {
            return NotFound();
        }

        return Ok(note);
    }

    [HttpGet("document/{documentId:long}")]
    public async Task<IActionResult> GetByDocument(long documentId, CancellationToken cancellationToken)
    {
        IEnumerable<NoteResponse> notes = await _noteService.GetNotesByDocumentAsync(documentId, cancellationToken);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        NoteResponse createdNote = await _noteService.CreateNoteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { externalId = createdNote.ExternalId }, createdNote);
    }

    [HttpPut("{externalId:guid}")]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        bool isUpdated = await _noteService.UpdateNoteAsync(externalId, request, cancellationToken);
        if (!isUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{externalId:guid}")]
    public async Task<IActionResult> Patch(Guid externalId, [FromBody] PatchNoteRequest request, CancellationToken cancellationToken)
    {
        bool isPatched = await _noteService.PatchNoteAsync(externalId, request, cancellationToken);
        if (!isPatched)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{externalId:guid}")]
    public async Task<IActionResult> Delete(Guid externalId, CancellationToken cancellationToken)
    {
        bool isDeleted = await _noteService.DeleteNoteAsync(externalId, cancellationToken);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
