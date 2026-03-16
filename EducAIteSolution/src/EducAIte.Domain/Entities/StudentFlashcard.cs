using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Entities;

public class StudentFlashcard
{
    // Surrogate Primary Key
    public long StudentFlashcardId { get; private set; }

    // The link to the Student (Who is studying?)
    public long StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    // The link to the Flashcard (What are they studying?)
    public long FlashcardId { get; private set; }
    public Flashcard Flashcard { get; private set; } = null!;

    // Performance Data
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int TotalAttempts => CorrectCount + WrongCount;
    public DateTime? LastReviewedAt { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public int ConsecutiveCorrectCount { get; private set; }

    public int ConsecutiveWrongCount {get; private set; }
    public int ReviewCount { get; private set; }
    public int LapseCount { get; private set; }
    public FlashcardReviewState State { get; private set; }
    public FlashcardReviewOutcome? LastReviewOutcome { get; private set; }

    // Additional Properties
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StudentFlashcard() { }

    public StudentFlashcard(long studentId, long flashcardId)
    {
        DateTime now = DateTime.UtcNow;
        StudentId = ValidatePositiveId(studentId, nameof(studentId));
        FlashcardId = ValidatePositiveId(flashcardId, nameof(flashcardId));
        CorrectCount = 0;
        ConsecutiveCorrectCount = 0;
        WrongCount = 0;
        NextReviewAt = now;
        State = FlashcardReviewState.New;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void StartTracking(DateTime? now = null)
    {
        EnsureNotDeleted();

        DateTime reviewedAt = NormalizeReviewTime(now);
        if (LastReviewedAt is null)
        {
            NextReviewAt = reviewedAt;
        }

        UpdatedAt = reviewedAt;
    }

    public void MarkCorrect()
    {
        ApplyCorrectReview(DateTime.UtcNow);
    }

    public void MarkWrong()
    {
        ApplyWrongReview(DateTime.UtcNow);
    }

    public void ApplyReviewResult(bool isCorrect, DateTime? reviewedAt = null)
    {
        if (isCorrect)
        {
            ApplyCorrectReview(reviewedAt);
            return;
        }

        ApplyWrongReview(reviewedAt);
    }

    public void ApplyCorrectReview(DateTime? reviewedAt = null)
    {
        EnsureNotDeleted();

        DateTime reviewTime = NormalizeReviewTime(reviewedAt);
        CorrectCount += 1;
        ReviewCount += 1;
        ConsecutiveCorrectCount += 1;
        ConsecutiveWrongCount = 0;
        LastReviewedAt = reviewTime;
        LastReviewOutcome = FlashcardReviewOutcome.Correct;
        State = DetermineStateAfterCorrect();
        NextReviewAt = CalculateNextReviewAtAfterCorrect(reviewTime);
        UpdatedAt = reviewTime;
    }

    public void ApplyWrongReview(DateTime? reviewedAt = null)
    {
        EnsureNotDeleted();

        DateTime reviewTime = NormalizeReviewTime(reviewedAt);
        WrongCount += 1;
        ReviewCount += 1;
        LapseCount += 1;
        ConsecutiveCorrectCount = 0;
        ConsecutiveWrongCount += 1;
        LastReviewedAt = reviewTime;
        LastReviewOutcome = FlashcardReviewOutcome.Wrong;
        State = State == FlashcardReviewState.Review ? FlashcardReviewState.Relearning : FlashcardReviewState.Learning;
        NextReviewAt = reviewTime.AddMinutes(5);
        UpdatedAt = reviewTime;
    }

    public bool IsDue(DateTime? now = null)
    {
        if (IsDeleted)
        {
            return false;
        }

        return NextReviewAt <= NormalizeReviewTime(now);
    }

    public void ResetProgress(DateTime? now = null)
    {
        EnsureNotDeleted();

        DateTime reviewTime = NormalizeReviewTime(now);
        CorrectCount = 0;
        WrongCount = 0;
        LastReviewedAt = null;
        NextReviewAt = reviewTime;
        ConsecutiveCorrectCount = 0;
        ConsecutiveWrongCount = 0;
        ReviewCount = 0;
        LapseCount = 0;
        State = FlashcardReviewState.New;
        LastReviewOutcome = null;
        UpdatedAt = reviewTime;
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

    public void Restore(DateTime? now = null)
    {
        if (!IsDeleted)
        {
            return;
        }

        DateTime reviewTime = NormalizeReviewTime(now);
        IsDeleted = false;
        if (NextReviewAt == default)
        {
            NextReviewAt = reviewTime;
        }

        UpdatedAt = reviewTime;
    }

    private FlashcardReviewState DetermineStateAfterCorrect()
    {
        if (ConsecutiveCorrectCount >= 2)
        {
            return FlashcardReviewState.Review;
        }

        return State == FlashcardReviewState.Relearning
            ? FlashcardReviewState.Learning
            : FlashcardReviewState.Learning;
    }

    private DateTime CalculateNextReviewAtAfterCorrect(DateTime reviewTime)
    {
        if (ConsecutiveCorrectCount <= 1)
        {
            return reviewTime.AddHours(12);
        }

        if (ConsecutiveCorrectCount == 2)
        {
            return reviewTime.AddDays(3);
        }

        int intervalDays = Math.Min(30, ConsecutiveCorrectCount * 2);
        return reviewTime.AddDays(intervalDays);
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted student flashcard.");
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

    private static DateTime NormalizeReviewTime(DateTime? reviewedAt)
    {
        return reviewedAt?.ToUniversalTime() ?? DateTime.UtcNow;
    }
}
