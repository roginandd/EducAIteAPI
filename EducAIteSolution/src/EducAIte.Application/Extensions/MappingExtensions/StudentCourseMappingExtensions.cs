using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
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
    /// <param name="sqidService">The sqid encoder.</param>
    /// <returns>The mapped response DTO.</returns>
    public static StudentCourseResponse ToResponse(this StudentCourse studentCourse, ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentCourse);
        ArgumentNullException.ThrowIfNull(sqidService);

        return studentCourse
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudentCourseResponse>();
    }

    /// <summary>
    /// Creates a new enrollment entity from the incoming request.
    /// </summary>
    /// <param name="request">The incoming API request.</param>
    /// <param name="courseId">The decoded course identifier.</param>
    /// <param name="studyLoadId">The decoded study load identifier.</param>
    /// <returns>A new <see cref="StudentCourse"/> instance.</returns>
    public static StudentCourse ToEntity(this CreateStudentCourseRequest request, long courseId, long studyLoadId) => new(courseId, studyLoadId);
}
