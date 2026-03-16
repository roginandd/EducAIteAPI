using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

public interface IFlashcardService
{
    Task<FlashcardResponse?> GetBySqidAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardResponse>> GetMineAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardResponse>> GetByDocumentAsync(string documentSqid, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardResponse>> GetByNoteAsync(string noteSqid, long studentId, CancellationToken cancellationToken = default);

    Task<FlashcardResponse> CreateAsync(CreateFlashcardRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FlashcardResponse>> CreateBulkAsync(CreateBulkFlashcardsRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(string flashcardSqid, UpdateFlashcardRequest request, long studentId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default);
}
