using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        DocumentResponse? document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        return Ok(document);
    }

    [HttpGet("student/{studentId:long}")]
    public async Task<IActionResult> GetByStudent(long studentId, CancellationToken cancellationToken)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByStudentAsync(studentId, cancellationToken);
            return Ok(documents);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createdDocument = await _documentService.CreateDocumentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdDocument.DocumentId }, createdDocument);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            bool isUpdated = await _documentService.UpdateDocumentAsync(id, request, cancellationToken);
            if (!isUpdated)
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        bool isDeleted = await _documentService.DeleteDocumentAsync(id, cancellationToken);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
