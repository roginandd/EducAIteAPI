using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
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
    /// Applies a study load update request to an existing entity.
    /// </summary>
    public static void ApplyToEntity(this StudyLoadUpdateRequest dto, StudyLoad studyLoad)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(studyLoad);

        studyLoad.SchoolYearStart = dto.SchoolYearStart;
        studyLoad.SchoolYearEnd = dto.SchoolYearEnd;
        studyLoad.Semester = NormalizeSemester(dto.Semester);
        studyLoad.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Maps a study load create request into the uploaded file metadata entity it owns.
    /// </summary>
    public static FileMetadata ToFileMetadataEntity(this StudyLoadCreateRequest dto, long studentId, string storageKey)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        return new FileMetadata
        {
            FileName = dto.StudyLoadDocument.FileName,
            FileExtension = Path.GetExtension(dto.StudyLoadDocument.FileName),
            ContentType = dto.StudyLoadDocument.ContentType,
            StorageKey = storageKey,
            FileSizeInBytes = dto.StudyLoadDocument.Length,
            UploadedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentId = studentId
        };
    }

    /// <summary>
    /// Applies a study load update request to the uploaded file metadata entity it owns.
    /// </summary>
    public static void ApplyToFileMetadataEntity(this StudyLoadUpdateRequest dto, FileMetadata fileMetadata, string storageKey)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(fileMetadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        fileMetadata.FileName = dto.StudyLoadDocument.FileName;
        fileMetadata.FileExtension = Path.GetExtension(dto.StudyLoadDocument.FileName);
        fileMetadata.ContentType = dto.StudyLoadDocument.ContentType;
        fileMetadata.StorageKey = storageKey;
        fileMetadata.FileSizeInBytes = dto.StudyLoadDocument.Length;
        fileMetadata.UpdatedAt = DateTime.UtcNow;
    }

    private static Semester NormalizeSemester(int semester)
    {
        if (!Enum.IsDefined(typeof(Semester), semester))
        {
            throw new ArgumentException("Semester is invalid.", nameof(semester));
        }

        return (Semester)semester;
    }

}
