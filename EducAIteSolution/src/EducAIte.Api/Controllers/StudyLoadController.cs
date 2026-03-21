using EducAIte.Application.DTOs;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudyLoadController : ControllerBase
{
    private readonly IStudyLoadService _studyLoadService;

    public StudyLoadController(IStudyLoadService studyLoadService)
    {
        _studyLoadService = studyLoadService;
    }

    /// <summary>
    /// Retrieves a study load for a specific student by student ID.
    /// </summary>
    /// <param name="studentSqid">The Sqid of the student.</param>
    /// <returns>The study load if found; otherwise, 404 Not Found.</returns>
    [HttpGet("student/{studentSqid}")]
    public async Task<IActionResult> GetByStudentId(string studentSqid)
    {
        var studyLoad = await _studyLoadService.GetAllStudyLoadsByStudentIdAsync(studentSqid);
        if (studyLoad is null)
        {
            return NoContent();
        }

        return Ok(studyLoad);
    }

    /// <summary>
    /// Creates a new study load for a student.
    /// </summary>
    /// <param name="request">The study load creation request.</param>
    /// <returns>The created study load with 201 Created status.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] StudyLoadCreateRequest request)
    {
        var createdStudyLoad = await _studyLoadService.AddStudyLoadAsync(request);
        return CreatedAtAction(nameof(GetByStudentId),
            new { studentSqid = createdStudyLoad.StudentSqid },
            createdStudyLoad);
    }

    /// <summary>
    /// Updates an existing study load.
    /// </summary>
    /// <param name="id">The ID of the study load to update.</param>
    /// <param name="request">The update request containing new values.</param>
    /// <returns>204 No Content on success; 404 Not Found if study load doesn't exist.</returns>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] StudyLoadUpdateRequest request)
    {
        return null;
    }

    /// <summary>
    /// Deletes a study load by ID.
    /// </summary>
    /// <param name="id">The ID of the study load to delete.</param>
    /// <returns>204 No Content on success; 404 Not Found if study load doesn't exist.</returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        return null;
    }
}
