using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EducAIte.Api.Controllers;

/// <summary>
/// Exposes student course enrollment and grade endpoints for the authenticated student.
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
    [HttpGet("{studentCourseSqid}")]
    public async Task<IActionResult> GetById(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            StudentCourseResponse? studentCourse = await _studentCourseService.GetByIdAsync(studentCourseSqid, studentId, cancellationToken);
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
    public async Task<IActionResult> GetMine([FromQuery] GetStudentCoursesRequest? request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<StudentCourseResponse> studentCourses = await _studentCourseService.GetMineAsync(studentId, request, cancellationToken);
            return Ok(studentCourses);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all enrollments for a specific study load owned by the authenticated student.
    /// </summary>
    [HttpGet("study-load/{studyLoadSqid}")]
    public async Task<IActionResult> GetByStudyLoad(string studyLoadSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<StudentCourseResponse> studentCourses = await _studentCourseService.GetByStudyLoadAsync(studyLoadSqid, studentId, cancellationToken);
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
    /// Retrieves all active grades for a student course owned by the authenticated student.
    /// </summary>
    [HttpGet("{studentCourseSqid}/grades")]
    public async Task<IActionResult> GetGrades(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<GradeResponseDTO> grades = await _studentCourseService.GetGradesAsync(studentCourseSqid, studentId, cancellationToken);
            return Ok(grades);
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
    /// Retrieves an active grade by grading period for a student course owned by the authenticated student.
    /// </summary>
    [HttpGet("{studentCourseSqid}/grades/{gradeType}")]
    public async Task<IActionResult> GetGradeByType(string studentCourseSqid, GradeType gradeType, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            GradeResponseDTO? grade = await _studentCourseService.GetGradeByTypeAsync(studentCourseSqid, gradeType, studentId, cancellationToken);
            if (grade is null)
            {
                return NotFound();
            }

            return Ok(grade);
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
    /// Retrieves active grades that match the requested grading periods for a student course.
    /// </summary>
    [HttpPost("grades/by-types")]
    public async Task<IActionResult> GetGradesByType([FromBody] GetGradesByTypeDTO request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            IReadOnlyList<GradeResponseDTO> grades = await _studentCourseService.GetGradesByTypeAsync(request, studentId, cancellationToken);
            return Ok(grades);
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
            return CreatedAtAction(nameof(GetById), new { studentCourseSqid = created.Sqid }, created);
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
    /// Creates or restores a grade for a student course owned by the authenticated student.
    /// </summary>
    [HttpPost("grades")]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeDTO request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            GradeResponseDTO created = await _studentCourseService.CreateGradeAsync(request, studentId, cancellationToken);
            return CreatedAtAction(
                nameof(GetGradeByType),
                new { studentCourseSqid = created.StudentCourseSqid, gradeType = created.GradeType },
                created);
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
    /// Updates an existing grade for a student course owned by the authenticated student.
    /// </summary>
    [HttpPut("grades")]
    public async Task<IActionResult> UpdateGrade([FromBody] UpdateGradeDTO request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            GradeResponseDTO updated = await _studentCourseService.UpdateGradeAsync(request, studentId, cancellationToken);
            return Ok(updated);
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
    [HttpDelete("{studentCourseSqid}")]
    public async Task<IActionResult> Delete(string studentCourseSqid, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            bool deleted = await _studentCourseService.DeleteAsync(studentCourseSqid, studentId, cancellationToken);
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

    /// <summary>
    /// Archives a grade for a student course owned by the authenticated student.
    /// </summary>
    [HttpDelete("{studentCourseSqid}/grades/{gradeType}")]
    public async Task<IActionResult> DeleteGrade(string studentCourseSqid, GradeType gradeType, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentStudentId(out long studentId))
        {
            return Unauthorized(new { message = "Student ID claim is missing or invalid." });
        }

        try
        {
            bool deleted = await _studentCourseService.DeleteGradeAsync(studentCourseSqid, gradeType, studentId, cancellationToken);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
