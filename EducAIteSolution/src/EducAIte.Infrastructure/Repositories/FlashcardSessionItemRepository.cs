using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class FlashcardSessionItemRepository : IFlashcardSessionItemRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardSessionItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FlashcardSessionItem>> GetBySessionIdAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessionItems
            .AsNoTracking()
            .Include(item => item.StudentFlashcard)
            .ThenInclude(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .ThenInclude(document => document.Folder)
            .Where(item => item.FlashcardSessionId == sessionId)
            .OrderBy(item => item.CurrentOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<FlashcardSessionItem?> GetTrackedByIdAsync(long sessionItemId, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardSessionItems
            .Include(item => item.StudentFlashcard)
            .ThenInclude(studentFlashcard => studentFlashcard.Flashcard)
            .ThenInclude(flashcard => flashcard.Note)
            .ThenInclude(note => note.Document)
            .ThenInclude(document => document.Folder)
            .FirstOrDefaultAsync(item => item.FlashcardSessionItemId == sessionItemId, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<FlashcardSessionItem> items, CancellationToken cancellationToken = default)
    {
        await _context.FlashcardSessionItems.AddRangeAsync(items, cancellationToken);
    }

    public async Task UpdateAsync(FlashcardSessionItem item, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
