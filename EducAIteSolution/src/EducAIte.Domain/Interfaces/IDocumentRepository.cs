using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> IsOwnedByStudentAsync(long documentId, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<long?> GetFolderStudentIdAsync(long folderId, CancellationToken cancellationToken = default);

    Task<long?> GetFileMetadataStudentIdAsync(long fileMetadataId, CancellationToken cancellationToken = default);
}
