using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;


public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetAllByDocumentIdAsync(long documentId, CancellationToken cancellationToken = default);

    Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default);

    Task UpdateAsync(Note note, CancellationToken cancellationToken = default);

    Task<Note?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

    Task<Note?> GetTrackedByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

}
