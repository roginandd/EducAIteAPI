using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.ValueObjects;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

/// <summary>
/// Static mapping extensions for the StudyLoad domain.
/// Provides high-performance, compile-time safe object transformation.
/// </summary>
public static class StudyLoadMappingExtensions
{
    /// <summary>
    /// Maps a StudyLoad entity to a StudyLoadResponse DTO.
    /// </summary>
    public static StudyLoadResponse ToResponse(this StudyLoad studyLoad, ISqidService sqidService)
    {
        return studyLoad
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudyLoadResponse>();
    }

    /// <summary>
    /// Maps a StudyLoad entity to a StudyLoadDto.
    /// </summary>
    public static StudyLoadDto ToDto(this StudyLoad studyLoad, ISqidService sqidService)
    {
        return studyLoad
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudyLoadDto>();
    }

    /// <summary>
    /// Maps a StudyLoadCreateRequest to a StudyLoad entity.
    /// </summary>
    public static StudyLoad ToEntity(this StudyLoadCreateRequest dto, long studentId) =>
        new()
        {
            StudentId = studentId,
            SchoolYearStart = dto.SchoolYearStart,
            SchoolYearEnd = dto.SchoolYearEnd,
            Semester = NormalizeSemester(dto.Semester),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// Maps a StudyLoadUpdateRequest into an existing StudyLoad entity.
    /// </summary>
    // public static StudyLoad ToEntity(this StudyLoadUpdateRequest dto, StudyLoad studyLoad)
    // {
    //     if (dto.SchoolYearStart.HasValue && dto.SchoolYearEnd.HasValue)
    //     {
    //         studyLoad.SchoolYearStart = dto.SchoolYearStart.Value;
    //         studyLoad.SchoolYearEnd = dto.SchoolYearEnd.Value;
    //     }

    //     if (dto.Semester.HasValue)
    //     {
    //         studyLoad.Semester = NormalizeSemester(dto.Semester.Value);
    //     }

    //     studyLoad.UpdatedAt = DateTime.UtcNow;
    //     return studyLoad;
    // }

    private static Semester NormalizeSemester(int semester)
    {
        if (!Enum.IsDefined(typeof(Semester), semester))
        {
            throw new ArgumentException("Semester is invalid.", nameof(semester));
        }

        return (Semester)semester;
    }

}
