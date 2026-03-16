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
            return await _context.FileMetadata.AsNoTracking().FirstOrDefaultAsync(fm => fm.FileMetadataId == id, cancellationToken);
        }
        public async Task<FileMetadata> AddFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
        {
            return await _context.FileMetadata.AddAsync(fileMetadata, cancellationToken);
        }
        public async Task<FileMetadata> UpdateFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.FileMetadata
                .Where(fm => fm.FileMetadataId == fileMetadata.FileMetadataId)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(f => f.FileName, fileMetadata.FileName)
                        .SetProperty(f => f.FileSize, fileMetadata.FileSize)
                        .SetProperty(f => f.ContentType, fileMetadata.ContentType),
                cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException($"FileMetadata with id {fileMetadata.FileMetadataId} not found.");

            return fileMetadata;
        }
        public async Task DeleteFileMetadataAsync(long id, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.FileMetadata
                .Where(fm => fm.FileMetadataId == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.IsDeleted, true),
                    cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException($"FileMetadata with id {id} not found.");
        }
    }
}