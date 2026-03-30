using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IFlashcardWorkspaceService
{
    Task<FlashcardWorkspaceLatestResponse> GetLatestWorkspaceAsync(
        long studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardDocumentResponse>> GetDocumentsAsync(
        string studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardResponse>> GetFlashcardsAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<FlashcardResponse> CreateFlashcardAsync(
        string documentSqid,
        CreateWorkspaceFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateFlashcardAsync(
        string documentSqid,
        string flashcardSqid,
        UpdateWorkspaceFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFlashcardAsync(
        string documentSqid,
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken = default);
}
