using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        return Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var createdCourse = await _courseService.CreateCourseAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdCourse.CourseId }, createdCourse);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] CreateBulkCoursesRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateCoursesBulkAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCourseRequest request)
    {
        var isUpdated = await _courseService.UpdateCourseAsync(id, request);
        if (!isUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var isDeleted = await _courseService.DeleteCourseAsync(id);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
