using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Mapping.Configurations;

public sealed class StudyLoadMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StudyLoad, StudyLoadResponse>()
            .Map(dest => dest.StudyLoadSqid, src => GetSqidService().Encode(src.StudyLoadId))
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.FileMetadataSqid, src => GetSqidService().Encode(src.FileMetadataId))
            .Map(dest => dest.Courses, src => src.Courses.Select(course => course.Adapt<CourseResponse>()).ToList())
            .Map(dest => dest.TotalUnits, src => src.TotalUnits);
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
