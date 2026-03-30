using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Folder?> GetTrackedByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Folder>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Folder>> GetAllByStudentIdAndSemesterAsync(
        long studentId,
        byte semester,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Folder>> GetAllByStudentIdAndSchoolYearAsync(
        long studentId,
        int schoolYearStart,
        int schoolYearEnd,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Folder>> GetSubFoldersAsync(
        long parentFolderId,
        CancellationToken cancellationToken = default);

    Task<Folder?> GetParentFolderAsync(long folderId, CancellationToken cancellationToken = default);

    Task<bool> IsOwnedByStudentAsync(long folderId, long studentId, CancellationToken cancellationToken = default);

    Task<bool> FolderKeyExistsAsync(
        long studentId,
        string folderKey,
        long? excludedFolderId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsDescendantAsync(
        long folderId,
        long potentialDescendantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetAllFolderKeysAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<long>> GetExistingStudentCourseIdsAsync(
        long studentId,
        IReadOnlyCollection<long> studentCourseIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(Folder folder, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Folder> folders, CancellationToken cancellationToken = default);

    Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
