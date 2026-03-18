using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EducAIte.Api.Controllers;

/// <summary>
/// Exposes student course enrollment endpoints for the authenticated student.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentCourseController : ControllerBase
{
    private readonly IStudentCourseService _studentCourseService;

    /// <summary>
    /// Initializes a new controller instance.
    /// </summary>
    /// <param name="studentCourseService">The student course service.</param>
    public StudentCourseController(IStudentCourseService studentCourseService)
    {
        _studentCourseService = studentCourseService;
    }

    /// <summary>
    /// Retrieves a specific enrollment owned by the authenticated student.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            StudentCourseResponse? studentCourse = await _studentCourseService.GetByIdAsync(id, studentId, cancellationToken);
            if (studentCourse is null)
            {
                return NotFound();
            }

            return Ok(studentCourse);
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

    /// <summary>
    /// Retrieves all enrollments for the authenticated student.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        IReadOnlyList<StudentCourseResponse> studentCourses = await _studentCourseService.GetMineAsync(studentId, cancellationToken);
        return Ok(studentCourses);
    }

    /// <summary>
    /// Retrieves all enrollments for a specific study load owned by the authenticated student.
    /// </summary>
    [HttpGet("study-load/{studyLoadId:long}")]
    public async Task<IActionResult> GetByStudyLoad(long studyLoadId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<StudentCourseResponse> studentCourses = await _studentCourseService.GetByStudyLoadAsync(studyLoadId, studentId, cancellationToken);
            return Ok(studentCourses);
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

    /// <summary>
    /// Creates a new enrollment for the authenticated student.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentCourseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            StudentCourseResponse created = await _studentCourseService.CreateAsync(request, studentId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.StudentCourseId }, created);
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
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Archives an enrollment owned by the authenticated student.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            bool deleted = await _studentCourseService.DeleteAsync(id, studentId, cancellationToken);
            if (!deleted)
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

    private bool TryGetCurrentStudentId(out long studentId)
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(claimValue, out studentId);
    }
}
