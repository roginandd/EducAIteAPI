using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IFolderService
{
    Task<FolderResponse?> GetFolderByIdAsync(string sqid, long studentId, CancellationToken cancellationToken = default);

    Task<FolderContentsResponse?> GetContentsAsync(string sqid, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FolderResponse>> GetFoldersByStudentAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FolderResponse>> GetFoldersBySemesterAsync(
        long studentId,
        byte semester,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FolderResponse>> GetFoldersBySchoolYearAsync(
        long studentId,
        int schoolYearStart,
        int schoolYearEnd,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FolderResponse>> GetSubFoldersAsync(
        string parentFolderSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<FolderResponse?> GetParentFolderAsync(
        string folderSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<FolderResponse> CreateFolderAsync(CreateFolderRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<bool> UpdateFolderAsync(
        string sqid,
        UpdateFolderRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFolderAsync(string sqid, long studentId, CancellationToken cancellationToken = default);
}
