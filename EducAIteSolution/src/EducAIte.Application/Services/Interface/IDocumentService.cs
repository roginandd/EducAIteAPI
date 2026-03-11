using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IDocumentService
{
    Task<DocumentResponse?> GetDocumentByIdAsync(string sqid, CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(long studentId, CancellationToken cancellationToken = default);

    Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateDocumentAsync(string sqid, UpdateDocumentRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteDocumentAsync(string sqid, CancellationToken cancellationToken = default);
}
