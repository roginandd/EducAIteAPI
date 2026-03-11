using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface INoteService
{
    Task<NoteResponse?> GetNoteBySqidAsync(string sqid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> GetNotesByDocumentAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> GetNotesByStudentAsync(long studentId, CancellationToken cancellationToken = default);

    Task<NoteResponse> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateNoteAsync(string sqid, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> PatchNoteAsync(string sqid, PatchNoteRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteNoteAsync(string sqid, CancellationToken cancellationToken = default);
}
