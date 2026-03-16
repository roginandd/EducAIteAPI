namespace EducAIte.Application.Interfaces;

using EducAIte.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICourseRepository Courses { get; }
    IDocumentRepository Documents { get; }
    IFlashcardRepository Flashcards { get; }
    INoteRepository Notes { get; }
    IStudentRepository Students { get; }
    IStudentFlashcardRepository StudentFlashcards { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
