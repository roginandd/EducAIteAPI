namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using global::Mapster;

/// <summary>
/// Mapster configuration class for Course-related mappings.
/// </summary>
public sealed class CourseMappingConfig : IRegister
{   

    /// <summary>
    /// Registers Mapster configurations for Course-related mappings.
    /// </summary>
    /// <param name="config"></param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Course, CourseResponse>();

        config.NewConfig<CreateCourseRequest, Course>()
            .Ignore(dest => dest.CourseId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted);

        config.NewConfig<UpdateCourseRequest, Course>()
            .Ignore(dest => dest.CourseId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted);
    }
}
