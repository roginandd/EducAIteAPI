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
    /// <param name="studentId">The ID of the student.</param>
    /// <returns>The study load if found; otherwise, 404 Not Found.</returns>
    [HttpGet("student/{studentId:long}")]
    public async Task<IActionResult> GetByStudentId(long studentId)
    {
        try
        {
            var studyLoad = await _studyLoadService.GetStudyLoadByStudentIdAsync(studentId);
            if (studyLoad is null)
            {
                return NotFound(new { message = $"Study load for student ID {studentId} not found." });
            }

            return Ok(studyLoad);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new study load for a student.
    /// </summary>
    /// <param name="request">The study load creation request.</param>
    /// <returns>The created study load with 201 Created status.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudyLoadCreateRequest request)
    {
        try
        {
            var createdStudyLoad = await _studyLoadService.AddStudyLoadAsync(request);
            return CreatedAtAction(nameof(GetByStudentId), 
                new { studentId = createdStudyLoad.StudentId }, 
                createdStudyLoad);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
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
        try
        {
            var updatedStudyLoad = await _studyLoadService.UpdateStudyLoadAsync(id, request);
            return Ok(updatedStudyLoad);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a study load by ID.
    /// </summary>
    /// <param name="id">The ID of the study load to delete.</param>
    /// <returns>204 No Content on success; 404 Not Found if study load doesn't exist.</returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var isDeleted = await _studyLoadService.DeleteStudyLoadAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = $"Study load with ID {id} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}
