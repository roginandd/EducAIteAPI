using EducAIte.Application.Interfaces;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace EducAIte.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        ApplicationDbContext context,
        ICourseRepository courses,
        IDocumentRepository documents,
        IFlashcardRepository flashcards,
        INoteRepository notes,
        IStudentRepository students,
        IStudentFlashcardRepository studentFlashcards)
    {
        _context = context;
        Courses = courses;
        Documents = documents;
        Flashcards = flashcards;
        Notes = notes;
        Students = students;
        StudentFlashcards = studentFlashcards;
    }

    public ICourseRepository Courses { get; }

    public IDocumentRepository Documents { get; }

    public IFlashcardRepository Flashcards { get; }

    public INoteRepository Notes { get; }

    public IStudentRepository Students { get; }

    public IStudentFlashcardRepository StudentFlashcards { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _currentTransaction?.Dispose();
        _currentTransaction = null;
        _disposed = true;
    }
}
