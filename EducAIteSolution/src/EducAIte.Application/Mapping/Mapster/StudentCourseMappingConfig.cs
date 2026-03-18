namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
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
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.StudentCourseId))
            .Map(dest => dest.CourseSqid, src => GetSqidService().Encode(src.CourseId))
            .Map(dest => dest.StudyLoadSqid, src => GetSqidService().Encode(src.StudyLoadId))
            .Map(dest => dest.EDPCode, src => src.Course.EDPCode)
            .Map(dest => dest.CourseName, src => src.Course.CourseName)
            .Map(dest => dest.Units, src => src.Course.Units);
    }

    private static ISqidService GetSqidService()
    {
        if (MapContext.Current?.Parameters.TryGetValue("sqidService", out object? value) != true || value is not ISqidService sqidService)
        {
            throw new InvalidOperationException("sqidService mapping parameter is required.");
        }

        return sqidService;
    }
}
