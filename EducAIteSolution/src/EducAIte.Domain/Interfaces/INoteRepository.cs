using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;


public interface INoteRepository
{
    Task<bool> IsOwnedByStudentAsync(long noteId, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Note>> GetAllByDocumentIdAndStudentIdAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Note>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Note>> GetAllByDocumentIdsAndStudentIdAsync(
        IReadOnlyCollection<long> documentIds,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Note>> SearchByFolderIdsAndStudentIdAsync(
        IReadOnlyCollection<long> folderIds,
        long studentId,
        string query,
        CancellationToken cancellationToken = default);

    Task<Note?> GetLastByDocumentIdAsync(long documentId, CancellationToken cancellationToken = default);

    Task AddAsync(Note note, CancellationToken cancellationToken = default);

    Task UpdateAsync(Note note, CancellationToken cancellationToken = default);

    Task<Note?> GetByIdAsync(long noteId, CancellationToken cancellationToken = default);

    Task<Note?> GetTrackedByIdAsync(long noteId, CancellationToken cancellationToken = default);

    Task<Note?> GetTrackedByIdAndDocumentIdAsync(long noteId, long documentId, CancellationToken cancellationToken = default);

    Task DeleteAsync(long noteId, CancellationToken cancellationToken = default);

    Task RebalanceDocumentAsync(long documentId, decimal step, CancellationToken cancellationToken = default);
}
