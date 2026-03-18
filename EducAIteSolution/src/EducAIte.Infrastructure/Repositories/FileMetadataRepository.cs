using EducAIte.Infrastructure.Data;
using EducAIte.Domain.Interfaces;
using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories
{
    public class FileMetadataRepository(ApplicationDbContext context) : IFileMetadataRepository
    {
        ApplicationDbContext _context = context;

        public async Task<FileMetadata?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata.AsNoTracking().FirstOrDefaultAsync(fm => fm.FileMetaDataId == id && !fm.IsDeleted, cancellationToken);
        }
        public async Task<FileMetadata> AddFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
        {
            var entry = await _context.FileMetadata.AddAsync(fileMetadata, cancellationToken);
            
            return entry.Entity;
        }
        public async Task<FileMetadata> UpdateFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.FileMetadata
                .Where(fm => fm.FileMetaDataId == fileMetadata.FileMetaDataId && !fm.IsDeleted)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(f => f.FileName, fileMetadata.FileName)
                        .SetProperty(f => f.FileSizeInBytes, fileMetadata.FileSizeInBytes)
                        .SetProperty(f => f.ContentType, fileMetadata.ContentType),
                cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException($"FileMetadata with id {fileMetadata.FileMetaDataId} not found.");

            return fileMetadata;
        }
        public async Task DeleteFileMetadataAsync(long id, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.FileMetadata
                .Where(fm => fm.FileMetaDataId == id && !fm.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.IsDeleted, true),
                    cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException($"FileMetadata with id {id} not found.");
        }
    }
}