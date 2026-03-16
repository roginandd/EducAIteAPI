using EducAIte.Application.DTOs;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.DTOs.Request;
using EducAIte.Domain.Entities;
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
    public static StudyLoadResponse ToResponse(this StudyLoad studyLoad) => studyLoad.Adapt<StudyLoadResponse>();

    /// <summary>
    /// Maps a StudyLoad entity to a StudyLoadResponse DTO.
    /// </summary>
    public static StudyLoadResponse ToDto(this StudyLoad studyLoad) => studyLoad.Adapt<StudyLoadResponse>();

    /// <summary>
    /// Maps a StudyLoadCreateRequest to a StudyLoad entity.
    /// </summary>
    public static StudyLoad ToEntity(this StudyLoadCreateRequest dto) =>
        new()
        {
            StudentId = dto.StudentId,
            SchoolYear = new SchoolYear(dto.SchoolYearStart, dto.SchoolYearEnd),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// Maps a StudyLoadUpdateRequest into an existing StudyLoad entity.
    /// </summary>
    public static StudyLoad ToEntity(this StudyLoadUpdateRequest dto, StudyLoad studyLoad)
    {
        if (dto.SchoolYearStart.HasValue && dto.SchoolYearEnd.HasValue)
        {
            studyLoad.SchoolYear = new SchoolYear(dto.SchoolYearStart.Value, dto.SchoolYearEnd.Value);
        }

        studyLoad.UpdatedAt = DateTime.UtcNow;
        return studyLoad;
    }

    public static List<StudyLoadResponse> ToDtoList(this IEnumerable<StudyLoad> studyLoads) => studyLoads.Select(s => s.ToDto()).ToList();
}
