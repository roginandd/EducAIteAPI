namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.ValueObjects;
using Mapster;

public sealed class FolderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Folder, FolderResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.FolderId))
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.SchoolYearStart, src => src.SchoolYear.StartYear)
            .Map(dest => dest.SchoolYearEnd, src => src.SchoolYear.EndYear)
            .Map(dest => dest.SchoolYearDisplayName, src => src.SchoolYear.DisplayName)
            .Map(dest => dest.StudentCourseSqid, src => GetSqidService().Encode(src.StudentCourseId))
            .Map(dest => dest.ParentFolderSqid, src => src.ParentFolderId.HasValue ? GetSqidService().Encode(src.ParentFolderId.Value) : null);

        config.NewConfig<CreateFolderRequest, Folder>()
            .ConstructUsing(src => new Folder(
                GetStudentId(),
                new SchoolYear(src.SchoolYearStart, src.SchoolYearEnd),
                src.Semester,
                src.FolderKey,
                src.Name,
                GetStudentCourseId()));

        config.NewConfig<UpdateFolderRequest, Folder>()
            .Ignore(destination => destination.FolderId)
            .Ignore(destination => destination.StudentId)
            .Ignore(destination => destination.Student)
            .Ignore(destination => destination.StudentCourse)
            .Ignore(destination => destination.ParentFolder)
            .Ignore(destination => destination.SubFolders)
            .Ignore(destination => destination.Documents)
            .Ignore(destination => destination.IsDeleted)
            .Ignore(destination => destination.CreatedAt)
            .Ignore(destination => destination.UpdatedAt);
    }

    private static ISqidService GetSqidService()
    {
        if (MapContext.Current?.Parameters.TryGetValue("sqidService", out object? value) != true || value is not ISqidService sqidService)
        {
            throw new InvalidOperationException("sqidService mapping parameter is required.");
        }

        return sqidService;
    }

    private static long GetStudentId()
    {
        if (MapContext.Current?.Parameters.TryGetValue("studentId", out object? value) != true || value is not long studentId)
        {
            throw new InvalidOperationException("studentId mapping parameter is required.");
        }

        return studentId;
    }

    private static long GetStudentCourseId()
    {
        if (MapContext.Current?.Parameters.TryGetValue("studentCourseId", out object? value) != true || value is not long studentCourseId)
        {
            throw new InvalidOperationException("studentCourseId mapping parameter is required.");
        }

        return studentCourseId;
    }
}
