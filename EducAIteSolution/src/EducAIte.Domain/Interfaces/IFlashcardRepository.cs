using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IFlashcardRepository
{
    Task<Flashcard?> GetByIdAsync(long flashcardId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(long flashcardId, CancellationToken cancellationToken = default);

    Task<Flashcard?> GetByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default);

    Task<Flashcard?> GetTrackedByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Flashcard>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Flashcard>> GetAllByDocumentIdAndStudentIdAsync(long documentId, long studentId, CancellationToken cancellationToken = default);

    Task<Flashcard> AddAsync(Flashcard flashcard, CancellationToken cancellationToken = default);

    Task UpdateAsync(Flashcard flashcard, CancellationToken cancellationToken = default);
}
