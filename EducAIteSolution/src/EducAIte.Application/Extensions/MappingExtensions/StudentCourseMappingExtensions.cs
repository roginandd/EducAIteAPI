using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

/// <summary>
/// Provides mapping helpers for student course enrollments.
/// </summary>
public static class StudentCourseMappingExtensions
{
    /// <summary>
    /// Maps an enrollment entity to its API response.
    /// </summary>
    /// <param name="studentCourse">The enrollment entity.</param>
    /// <returns>The mapped response DTO.</returns>
    public static StudentCourseResponse ToResponse(this StudentCourse studentCourse) => studentCourse.Adapt<StudentCourseResponse>();

    /// <summary>
    /// Creates a new enrollment entity from the incoming request.
    /// </summary>
    /// <param name="request">The incoming API request.</param>
    /// <returns>A new <see cref="StudentCourse"/> instance.</returns>
    public static StudentCourse ToEntity(this CreateStudentCourseRequest request) => new(request.CourseId, request.StudyLoadId);
}
