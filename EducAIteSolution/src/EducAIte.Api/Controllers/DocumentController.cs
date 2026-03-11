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
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{sqid}")]
    public async Task<IActionResult> GetById(string sqid, CancellationToken cancellationToken)
    {
        DocumentResponse? document = await _documentService.GetDocumentByIdAsync(sqid, cancellationToken);
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

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IEnumerable<DocumentResponse> documents = await _documentService.GetDocumentsByStudentAsync(studentId, cancellationToken);
        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createdDocument = await _documentService.CreateDocumentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { sqid = createdDocument.Sqid }, createdDocument);
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

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            bool isUpdated = await _documentService.UpdateDocumentAsync(sqid, request, cancellationToken);
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

    [HttpDelete("{sqid}")]
    public async Task<IActionResult> Delete(string sqid, CancellationToken cancellationToken)
    {
        bool isDeleted = await _documentService.DeleteDocumentAsync(sqid, cancellationToken);
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
