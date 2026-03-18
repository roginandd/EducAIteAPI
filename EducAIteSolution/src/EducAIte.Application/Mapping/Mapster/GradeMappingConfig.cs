using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Mapping.Configurations;

/// <summary>
/// Registers Mapster configuration for grade mappings that depend on sqid conversion.
/// </summary>
public sealed class GradeMappingConfig : IRegister
{
    /// <summary>
    /// Registers the grade mapping rules.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Grade, GradeResponseDTO>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.GradeId))
            .Map(dest => dest.StudentCourseSqid, src => GetSqidService().Encode(src.StudentCourseId))
            .Map(dest => dest.IsPassing, src => src.IsPassing());
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
