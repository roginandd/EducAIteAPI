namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class FlashcardSessionItem
{
    public long FlashcardSessionItemId { get; private set; }
    public long FlashcardSessionId { get; private set; }
    public FlashcardSession FlashcardSession { get; private set; } = null!;
    public long StudentFlashcardId { get; private set; }
    public StudentFlashcard StudentFlashcard { get; private set; } = null!;
    public int OriginalOrder { get; private set; }
    public int CurrentOrder { get; private set; }
    public FlashcardSessionItemStatus Status { get; private set; }
    public DateTime? PresentedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public long? LastAnswerHistoryId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private FlashcardSessionItem()
    {
    }

    public FlashcardSessionItem(long flashcardSessionId, long studentFlashcardId, int originalOrder)
    {
        FlashcardSessionId = ValidatePositiveId(flashcardSessionId, nameof(flashcardSessionId));
        StudentFlashcardId = ValidatePositiveId(studentFlashcardId, nameof(studentFlashcardId));
        OriginalOrder = Math.Max(0, originalOrder);
        CurrentOrder = OriginalOrder;
        Status = FlashcardSessionItemStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPresented(DateTime at)
    {
        EnsureNotDeleted();
        PresentedAt = at.ToUniversalTime();
        Status = FlashcardSessionItemStatus.Presented;
        UpdatedAt = PresentedAt.Value;
    }

    public void MarkAnswered(long? answerHistoryId, DateTime at)
    {
        EnsureNotDeleted();
        LastAnswerHistoryId = answerHistoryId is > 0 ? answerHistoryId.Value : null;
        AnsweredAt = at.ToUniversalTime();
        Status = FlashcardSessionItemStatus.Answered;
        UpdatedAt = AnsweredAt.Value;
    }

    public void RequeueLater(int newOrder)
    {
        EnsureNotDeleted();
        CurrentOrder = Math.Max(CurrentOrder + 1, newOrder);
        Status = FlashcardSessionItemStatus.Requeued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted flashcard session item.");
        }
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }
}
