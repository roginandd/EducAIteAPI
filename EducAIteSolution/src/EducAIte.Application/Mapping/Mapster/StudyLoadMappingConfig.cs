namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

/// <summary>
/// Registers StudyLoad mappings that depend on sqid conversion.
/// </summary>
public sealed class StudyLoadMappingConfig : IRegister
{
    /// <summary>
    /// Registers the study load mappings.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StudyLoad, StudyLoadResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.StudyLoadId))
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.FileMetadataSqid, src => src.FileMetadataId > 0 ? GetSqidService().Encode(src.FileMetadataId) : string.Empty)
            .Map(dest => dest.SchoolYearStart, src => src.SchoolYearStart)
            .Map(dest => dest.SchoolYearEnd, src => src.SchoolYearEnd)
            .Map(dest => dest.Semester, src => src.Semester.ToString());

        config.NewConfig<StudyLoad, StudyLoadDto>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.StudyLoadId))
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.FileMetadataSqid, src => src.FileMetadataId > 0 ? GetSqidService().Encode(src.FileMetadataId) : string.Empty)
            .Map(dest => dest.SchoolYearStart, src => src.SchoolYearStart)
            .Map(dest => dest.SchoolYearEnd, src => src.SchoolYearEnd)
            .Map(dest => dest.Semester, src => src.Semester.ToString());
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
