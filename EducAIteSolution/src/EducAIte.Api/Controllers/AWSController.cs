using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AWSController(IAWSService awsService) : ControllerBase
{
    private readonly IAWSService _awsService = awsService;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var key = await _awsService.UploadFileAsync(file, cancellationToken);
        return Ok(new { key });
    }

    [HttpGet("signed-url")]
    public IActionResult GetSignedUrl([FromQuery] string key, [FromQuery] int expiresInMinutes = 60, CancellationToken cancellationToken = default)
    {
        var url = _awsService.GenerateSignedUrl(key, TimeSpan.FromMinutes(expiresInMinutes), cancellationToken);
        return Ok(new { url });
    }
}
