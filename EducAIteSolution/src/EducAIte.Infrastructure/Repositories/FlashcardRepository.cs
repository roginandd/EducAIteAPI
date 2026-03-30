using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class FlashcardRepository : IFlashcardRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Flashcard?> GetByIdAsync(long flashcardId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(flashcard => flashcard.FlashcardId == flashcardId, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long flashcardId, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .AsNoTracking()
            .AnyAsync(flashcard => flashcard.FlashcardId == flashcardId, cancellationToken);
    }

    public async Task<Flashcard?> GetByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(flashcard => flashcard.FlashcardId == flashcardId)
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Flashcard?> GetTrackedByIdAndStudentIdAsync(long flashcardId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .Where(flashcard => flashcard.FlashcardId == flashcardId)
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetAllByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .OrderByDescending(flashcard => flashcard.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetAllByDocumentIdAndStudentIdAsync(long documentId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(flashcard => flashcard.Note.DocumentId == documentId)
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .OrderByDescending(flashcard => flashcard.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetAllByNoteIdAndStudentIdAsync(long noteId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(flashcard => flashcard.NoteId == noteId)
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .OrderByDescending(flashcard => flashcard.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Flashcard>> GetUntrackedByStudentIdAsync(
        long studentId,
        IReadOnlyCollection<long> excludeFlashcardIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Flashcard> query = CreateBaseQuery()
            .AsNoTracking()
            .Where(flashcard => flashcard.Note.Document.Folder.StudentId == studentId)
            .Where(flashcard => !_context.StudentFlashcards
                .IgnoreQueryFilters()
                .Any(studentFlashcard => studentFlashcard.StudentId == studentId && studentFlashcard.FlashcardId == flashcard.FlashcardId));

        if (excludeFlashcardIds.Count > 0)
        {
            query = query.Where(flashcard => !excludeFlashcardIds.Contains(flashcard.FlashcardId));
        }

        return await query
            .OrderBy(flashcard => flashcard.FlashcardId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Flashcard> AddAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.Add(flashcard);
        await _context.SaveChangesAsync(cancellationToken);
        return flashcard;
    }

    public async Task AddRangeAsync(IEnumerable<Flashcard> flashcards, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.AddRange(flashcards);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Flashcard flashcard, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Flashcard> CreateBaseQuery()
    {
        return _context.Flashcards
            .Include(flashcard => flashcard.AcceptedAnswerAliases)
            .Include(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .ThenInclude(document => document.Folder)
            .Where(flashcard => !flashcard.IsDeleted)
            .Where(flashcard => !flashcard.Note.IsDeleted)
            .Where(flashcard => !flashcard.Note.Document.IsDeleted)
            .Where(flashcard => !flashcard.Note.Document.Folder.IsDeleted)
            .Where(flashcard => !flashcard.Note.Document.FileMetadata.IsDeleted);
    }
}
