using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FolderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Folder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(folder => folder.FolderId == id && !folder.IsDeleted, cancellationToken);
    }

    public async Task<Folder?> GetTrackedByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .FirstOrDefaultAsync(folder => folder.FolderId == id && !folder.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.StudentId == studentId && !folder.IsDeleted)
            .OrderBy(folder => folder.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetAllByStudentIdAndSemesterAsync(
        long studentId,
        byte semester,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.StudentId == studentId && folder.Semester == semester && !folder.IsDeleted)
            .OrderBy(folder => folder.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetAllByStudentIdAndSchoolYearAsync(
        long studentId,
        int schoolYearStart,
        int schoolYearEnd,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .Where(folder =>
                folder.StudentId == studentId &&
                folder.SchoolYear.StartYear == schoolYearStart &&
                folder.SchoolYear.EndYear == schoolYearEnd &&
                !folder.IsDeleted)
            .OrderBy(folder => folder.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetSubFoldersAsync(
        long parentFolderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.ParentFolderId == parentFolderId && !folder.IsDeleted)
            .OrderBy(folder => folder.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Folder?> GetParentFolderAsync(long folderId, CancellationToken cancellationToken = default)
    {
        long? parentFolderId = await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.FolderId == folderId && !folder.IsDeleted)
            .Select(folder => folder.ParentFolderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!parentFolderId.HasValue)
        {
            return null;
        }

        return await _dbContext.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(folder => folder.FolderId == parentFolderId.Value && !folder.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsOwnedByStudentAsync(long folderId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .AnyAsync(folder => folder.FolderId == folderId && folder.StudentId == studentId && !folder.IsDeleted, cancellationToken);
    }

    public async Task<bool> FolderKeyExistsAsync(
        long studentId,
        string folderKey,
        long? excludedFolderId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Folder> query = _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.StudentId == studentId && folder.FolderKey == folderKey && !folder.IsDeleted);

        if (excludedFolderId.HasValue)
        {
            query = query.Where(folder => folder.FolderId != excludedFolderId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsDescendantAsync(
        long folderId,
        long potentialDescendantId,
        CancellationToken cancellationToken = default)
    {
        long? currentParentId = potentialDescendantId;

        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == folderId)
            {
                return true;
            }

            currentParentId = await _dbContext.Folders
                .AsNoTracking()
                .Where(folder => folder.FolderId == currentParentId.Value && !folder.IsDeleted)
                .Select(folder => folder.ParentFolderId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }

    public async Task<IReadOnlySet<string>> GetAllFolderKeysAsync(long studentId, CancellationToken cancellationToken = default)
    {
        List<string> folderKeys = await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.StudentId == studentId && !folder.IsDeleted)
            .Select(folder => folder.FolderKey)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(folderKeys, StringComparer.Ordinal);
    }

    public async Task<IReadOnlySet<long>> GetExistingStudentCourseIdsAsync(
        long studentId,
        IReadOnlyCollection<long> studentCourseIds,
        CancellationToken cancellationToken = default)
    {
        if (studentCourseIds.Count == 0)
        {
            return new HashSet<long>();
        }

        List<long> existingCourseIds = await _dbContext.Folders
            .AsNoTracking()
            .Where(folder =>
                folder.StudentId == studentId &&
                studentCourseIds.Contains(folder.StudentCourseId) &&
                !folder.IsDeleted)
            .Select(folder => folder.StudentCourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<long>(existingCourseIds);
    }

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        await _dbContext.Folders.AddAsync(folder, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Folder> folders, CancellationToken cancellationToken = default)
    {
        await _dbContext.Folders.AddRangeAsync(folders, cancellationToken);
    }

    public async Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        Folder? existingFolder = await GetTrackedByIdAsync(folder.FolderId, cancellationToken);
        if (existingFolder is null)
        {
            return;
        }

        existingFolder.UpdateDetails(
            folder.SchoolYear,
            folder.Semester,
            folder.FolderKey,
            folder.Name,
            folder.StudentCourseId,
            folder.ParentFolderId);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        Folder? rootFolder = await _dbContext.Folders
            .FirstOrDefaultAsync(folder => folder.FolderId == id && !folder.IsDeleted, cancellationToken);

        if (rootFolder is null)
        {
            return;
        }

        List<Folder> foldersToDelete = [rootFolder];
        Queue<long> queue = new([rootFolder.FolderId]);

        while (queue.Count > 0)
        {
            long currentFolderId = queue.Dequeue();

            List<Folder> childFolders = await _dbContext.Folders
                .Where(folder => folder.ParentFolderId == currentFolderId && !folder.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (Folder childFolder in childFolders)
            {
                foldersToDelete.Add(childFolder);
                queue.Enqueue(childFolder.FolderId);
            }
        }

        long[] folderIds = foldersToDelete.Select(folder => folder.FolderId).ToArray();
        List<Document> documents = await _dbContext.Documents
            .Where(document => folderIds.Contains(document.FolderId) && !document.IsDeleted)
            .Include(document => document.Notes)
            .ThenInclude(note => note.Flashcards)
            .ToListAsync(cancellationToken);

        foreach (Folder folder in foldersToDelete.OrderByDescending(folder => folder.ParentFolderId.HasValue))
        {
            IReadOnlyList<Folder> directChildren = foldersToDelete
                .Where(candidate => candidate.ParentFolderId == folder.FolderId)
                .ToList();

            IReadOnlyList<Document> folderDocuments = documents
                .Where(document => document.FolderId == folder.FolderId)
                .ToList();

            folder.MarkDeletedWithChildren(directChildren, folderDocuments);
        }
    }
}
