using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AWSController(IAWSService awsService) : ControllerBase
{
    private readonly IAWSService _awsService = awsService;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? path, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var uploadPath = string.IsNullOrWhiteSpace(path)
            ? $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"
            : path;

        var key = await _awsService.UploadFileAsync(file, uploadPath, cancellationToken);
        return Ok(new { key });
    }

    [HttpGet("signed-url")]
    public IActionResult GetSignedUrl([FromQuery] string key, [FromQuery] int expiresInMinutes = 60, CancellationToken cancellationToken = default)
    {
        var url = _awsService.GenerateSignedUrl(key, TimeSpan.FromMinutes(expiresInMinutes), cancellationToken);
        return Ok(new { url });
    }

    [HttpGet("signed-url/note-context")]
    public IActionResult GetNoteContextSignedUrl([FromQuery] string key, [FromQuery] int expiresInMinutes = 60, CancellationToken cancellationToken = default)
    {
        var url = _awsService.GenerateNoteContextSignedUrl(key, TimeSpan.FromMinutes(expiresInMinutes), cancellationToken);
        return Ok(new { url });
    }

    [HttpGet("signed-url/note-image")]
    public IActionResult GetNoteImageSignedUrl([FromQuery] string key, [FromQuery] int expiresInMinutes = 60, CancellationToken cancellationToken = default)
    {
        var url = _awsService.GenerateNoteImageSignedUrl(key, TimeSpan.FromMinutes(expiresInMinutes), cancellationToken);
        return Ok(new { url });
    }

    [HttpGet("signed-url/study-load")]
    public IActionResult GetStudyLoadSignedUrl([FromQuery] string key, [FromQuery] int expiresInMinutes = 60, CancellationToken cancellationToken = default)
    {
        var url = _awsService.GenerateStudyLoadSignedUrl(key, TimeSpan.FromMinutes(expiresInMinutes), cancellationToken);
        return Ok(new { url });
    }
}
