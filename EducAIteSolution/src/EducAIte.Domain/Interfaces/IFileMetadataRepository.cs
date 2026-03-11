namespace EducAIte.Domain.Interfaces
{
    public interface IFileMetadataRepository
    {
        public Task<FileMetadata?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        public Task<FileMetadata> AddFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
        public Task<FileMetadata> UpdateFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
        public Task<bool> DeleteFileMetadataAsync(long id, CancellationToken cancellationToken = default);
    }
}