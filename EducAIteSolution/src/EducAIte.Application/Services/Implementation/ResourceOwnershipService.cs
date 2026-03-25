using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;

namespace EducAIte.Application.Services.Implementation;

public sealed class ResourceOwnershipService : IResourceOwnershipService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INoteRepository _noteRepository;

    public ResourceOwnershipService(
        IFolderRepository folderRepository,
        IDocumentRepository documentRepository,
        INoteRepository noteRepository)
    {
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
        _noteRepository = noteRepository;
    }

    public async Task EnsureFolderOwnedByStudentAsync(
        long folderId,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        bool isOwned = await _folderRepository.IsOwnedByStudentAsync(folderId, studentId, cancellationToken);
        if (!isOwned)
        {
            throw new UnauthorizedAccessException("Folder does not belong to the authenticated student.");
        }
    }

    public async Task EnsureDocumentOwnedByStudentAsync(
        long documentId,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        bool isOwned = await _documentRepository.IsOwnedByStudentAsync(documentId, studentId, cancellationToken);
        if (!isOwned)
        {
            throw new UnauthorizedAccessException("Document does not belong to the authenticated student.");
        }
    }

    public async Task EnsureNoteOwnedByStudentAsync(
        long noteId,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        bool isOwned = await _noteRepository.IsOwnedByStudentAsync(noteId, studentId, cancellationToken);
        if (!isOwned)
        {
            throw new UnauthorizedAccessException("Note does not belong to the authenticated student.");
        }
    }
}
