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
public class FolderController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FolderController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpGet("{sqid}")]
    public async Task<IActionResult> GetById(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FolderResponse? folder = await _folderService.GetFolderByIdAsync(sqid, studentId, cancellationToken);
        if (folder is null)
        {
            return NotFound();
        }

        return Ok(folder);
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

        IReadOnlyList<FolderResponse> folders = await _folderService.GetFoldersByStudentAsync(authenticatedStudentId, cancellationToken);
        return Ok(folders);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FolderResponse> folders = await _folderService.GetFoldersByStudentAsync(studentId, cancellationToken);
        return Ok(folders);
    }

    [HttpGet("me/semester/{semester}")]
    public async Task<IActionResult> GetMineBySemester(byte semester, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FolderResponse> folders = await _folderService.GetFoldersBySemesterAsync(studentId, semester, cancellationToken);
        return Ok(folders);
    }

    [HttpGet("me/school-year")]
    public async Task<IActionResult> GetMineBySchoolYear(
        [FromQuery] int startYear,
        [FromQuery] int endYear,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FolderResponse> folders = await _folderService.GetFoldersBySchoolYearAsync(
            studentId,
            startYear,
            endYear,
            cancellationToken);

        return Ok(folders);
    }

    [HttpGet("{sqid}/subfolders")]
    public async Task<IActionResult> GetSubFolders(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<FolderResponse> folders = await _folderService.GetSubFoldersAsync(sqid, studentId, cancellationToken);
        return Ok(folders);
    }

    [HttpGet("{sqid}/parent")]
    public async Task<IActionResult> GetParentFolder(string sqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FolderResponse? folder = await _folderService.GetParentFolderAsync(sqid, studentId, cancellationToken);
        if (folder is null)
        {
            return NotFound();
        }

        return Ok(folder);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFolderRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        FolderResponse createdFolder = await _folderService.CreateFolderAsync(request, studentId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { sqid = createdFolder.Sqid }, createdFolder);
    }

    [HttpPut("{sqid}")]
    public async Task<IActionResult> Update(string sqid, [FromBody] UpdateFolderRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        bool isUpdated = await _folderService.UpdateFolderAsync(sqid, request, studentId, cancellationToken);
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

        bool isDeleted = await _folderService.DeleteFolderAsync(sqid, studentId, cancellationToken);
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
