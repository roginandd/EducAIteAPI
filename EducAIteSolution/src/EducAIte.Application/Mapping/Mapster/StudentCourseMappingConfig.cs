namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

/// <summary>
/// Registers Mapster configuration for student course mappings.
/// </summary>
public sealed class StudentCourseMappingConfig : IRegister
{
    /// <summary>
    /// Registers the student course mapping rules.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StudentCourse, StudentCourseResponse>()
            .Map(dest => dest.EDPCode, src => src.Course.EDPCode)
            .Map(dest => dest.CourseName, src => src.Course.CourseName)
            .Map(dest => dest.Units, src => src.Course.Units);
    }
}
