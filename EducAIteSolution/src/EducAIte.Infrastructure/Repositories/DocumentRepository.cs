using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DocumentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Document?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .AsNoTracking()
            .Include(document => document.Folder)
            .Include(document => document.FileMetadata)
            .FirstOrDefaultAsync(document =>
                document.DocumentId == id &&
                !document.Folder.IsDeleted &&
                !document.FileMetadata.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .AsNoTracking()
            .Include(document => document.Folder)
            .Include(document => document.FileMetadata)
            .Where(document =>
                document.Folder.StudentId == studentId &&
                !document.Folder.IsDeleted &&
                !document.FileMetadata.IsDeleted)
            .OrderBy(document => document.DocumentName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(document.DocumentId, cancellationToken) ?? document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        Document? existingDocument = await _dbContext.Documents
            .FirstOrDefaultAsync(existing => existing.DocumentId == document.DocumentId, cancellationToken);

        if (existingDocument is null)
        {
            return;
        }

        existingDocument.UpdateDetails(document.DocumentName, document.FolderId, document.FileMetadataId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        Document? existingDocument = await _dbContext.Documents
            .FirstOrDefaultAsync(document => document.DocumentId == id, cancellationToken);

        if (existingDocument is null)
        {
            return;
        }

        existingDocument.MarkDeleted();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<long?> GetFolderStudentIdAsync(long folderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Folders
            .AsNoTracking()
            .Where(folder => folder.FolderId == folderId && !folder.IsDeleted)
            .Select(folder => (long?)folder.StudentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<long?> GetFileMetadataStudentIdAsync(long fileMetadataId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMetadata
            .AsNoTracking()
            .Where(file => file.FileMetaDataId == fileMetadataId && !file.IsDeleted)
            .Select(file => (long?)file.StudentId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
