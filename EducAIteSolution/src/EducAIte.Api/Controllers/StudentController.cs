using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] StudentRegistrationRequest request)
    {
        try
        {
            var registeredStudent = await _studentService.RegisterStudentAsync(request);
            return CreatedAtAction(nameof(GetById), new { studentId = registeredStudent.Id }, registeredStudent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{studentId:long}")]
    [Authorize]
    public async Task<IActionResult> GetById(long studentId)
    {
        try
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            return Ok(student);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
