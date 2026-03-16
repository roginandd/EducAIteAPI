namespace EducAIte.Application.Services.Interface;

public interface IResourceOwnershipService
{
    Task EnsureDocumentOwnedByStudentAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken = default);

    Task EnsureNoteOwnedByStudentAsync(
        long noteId,
        long studentId,
        CancellationToken cancellationToken = default);
}
