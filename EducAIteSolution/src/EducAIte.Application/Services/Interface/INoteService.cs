using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface INoteService
{
    Task<NoteResponse?> GetNoteByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteResponse>> GetNotesByDocumentAsync(long documentId, CancellationToken cancellationToken = default);

    Task<NoteResponse> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateNoteAsync(Guid externalId, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> PatchNoteAsync(Guid externalId, PatchNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteNoteAsync(Guid externalId, CancellationToken cancellationToken = default);
}
