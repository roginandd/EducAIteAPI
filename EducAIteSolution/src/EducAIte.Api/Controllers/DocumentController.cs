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
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        DocumentResponse? document = await _documentService.GetDocumentByIdAsync(sqid, studentId, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        return Ok(document);
    }

    [HttpGet("{sqid}/signed-url")]
    public async Task<IActionResult> GetSignedUrl(
        string sqid,
        [FromQuery] int expiresInMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        SignedUrlResponse? signedUrl = await _documentService.GetSignedUrlAsync(
            sqid,
            studentId,
            expiresInMinutes,
            cancellationToken);

        if (signedUrl is null)
        {
            return NotFound();
        }

        return Ok(signedUrl);
    }

    [HttpGet("student/{studentId:long}")]
    public async Task<IActionResult> GetByStudent(long studentId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long authenticatedStudentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        if (studentId != authenticatedStudentId)
        {
            return Forbid();
        }

        var documents = await _documentService.GetDocumentsByStudentAsync(authenticatedStudentId, cancellationToken);
        return Ok(documents);
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
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        var createdDocument = await _documentService.CreateDocumentAsync(request, studentId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { sqid = createdDocument.Sqid }, createdDocument);
    }

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isUpdated = await _documentService.UpdateDocumentAsync(sqid, request, studentId, cancellationToken);
        if (!isUpdated)
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

        bool isDeleted = await _documentService.DeleteDocumentAsync(sqid, studentId, cancellationToken);

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
