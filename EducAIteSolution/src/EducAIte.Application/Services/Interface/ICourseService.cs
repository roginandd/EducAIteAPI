using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

/// <summary>
/// Defines the Application Service layer for the Course domain.
/// Orchestrates business logic, validation, and domain mapping.
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Retrieves a specific course by its unique identity.
    /// </summary>
    /// <param name="id">The surrogate key of the course.</param>
    /// <returns>A CourseResponse if found; otherwise, null.</returns>
    Task<CourseResponse?> GetCourseByIdAsync(long id);

    /// <summary>
    /// Retrieves a collection of all non-deleted courses.
    /// </summary>
    /// <returns>A collection of CourseResponse objects.</returns>
    Task<IEnumerable<CourseResponse>> GetAllCoursesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and creates a new course record.
    /// </summary>
    /// <param name="request">The data required to create the course.</param>
    /// <returns>A CourseResponse representing the newly created state.</returns>
    Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request);

    /// <summary>
    /// Updates an existing course record after performing validation.
    /// </summary>
    /// <param name="id">The surrogate key of the course to update.</param>
    /// <param name="request">The updated attributes of the course.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateCourseAsync(long id, UpdateCourseRequest request);

    /// <summary>
    /// Initiates a soft delete for an existing course.
    /// </summary>
    /// <param name="id">The surrogate key of the course to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteCourseAsync(long id);
}