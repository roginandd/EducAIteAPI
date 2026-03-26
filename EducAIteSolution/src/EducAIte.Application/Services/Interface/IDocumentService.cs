using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IDocumentService
{
    Task<DocumentResponse?> GetDocumentByIdAsync(
        string sqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<SignedUrlResponse?> GetSignedUrlAsync(
        string sqid,
        long studentId,
        int expiresInMinutes = 60,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(long studentId, CancellationToken cancellationToken = default);

    Task<UploadFolderDocumentResponse> UploadToFolderAsync(
        string folderSqid,
        UploadFolderDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<DocumentResponse> CreateDocumentAsync(
        CreateDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateDocumentAsync(
        string sqid,
        UpdateDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteDocumentAsync(string sqid, long studentId, CancellationToken cancellationToken = default);
}
