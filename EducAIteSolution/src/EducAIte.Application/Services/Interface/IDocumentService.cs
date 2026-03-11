using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IDocumentService
{
    Task<DocumentResponse?> GetDocumentByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(long studentId, CancellationToken cancellationToken = default);

    Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateDocumentAsync(long id, UpdateDocumentRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteDocumentAsync(long id, CancellationToken cancellationToken = default);
}
