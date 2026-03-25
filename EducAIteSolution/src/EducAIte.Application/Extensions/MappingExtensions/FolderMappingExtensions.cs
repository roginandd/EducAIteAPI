using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.ValueObjects;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class FolderMappingExtensions
{
    public static FolderResponse ToResponse(this Folder folder, ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(sqidService);

        return folder
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<FolderResponse>();
    }

    public static Folder ToEntity(this CreateFolderRequest request, long studentId)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request
            .BuildAdapter()
            .AddParameters("studentId", studentId)
            .AdaptToType<Folder>();
    }

    public static void ApplyToEntity(this UpdateFolderRequest request, Folder folder)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(folder);

        folder.UpdateDetails(
            new SchoolYear(request.SchoolYearStart, request.SchoolYearEnd),
            request.Semester,
            request.FolderKey,
            request.Name,
            request.CourseId,
            folder.ParentFolderId);
    }
}
