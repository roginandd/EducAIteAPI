using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

/// <summary>
/// Static mapping extensions for the Course domain.
/// Provides high-performance, compile-time safe object transformation.
/// </summary>
public static class CourseMappingExtensions
{

    /// <summary>
    ///  Maps a Course entity to a CourseResponse DTO.
    /// </summary>
    /// <param name="course"></param>
    /// <returns></returns>
    public static CourseResponse ToResponse(this Course course) => course.Adapt<CourseResponse>();


    /// <summary>
    /// Maps a CreateCourseRequest DTO to a Course entity.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Course ToEntity(this CreateCourseRequest request) => request.Adapt<Course>();

    /// <summary>
    /// Maps an UpdateCourseRequest DTO into an existing Course entity.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="course"></param>
    /// <returns></returns>
    public static Course ToEntity(this UpdateCourseRequest request, Course course)
    {
        request.Adapt(course);
        course.UpdatedAt = DateTime.UtcNow;
        return course;
    }
}
