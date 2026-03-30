namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

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
    public FlashcardReviewOutcome? LastReviewOutcome { get; private set; }
    public FlashcardAnswerVerdict? LastEvaluationVerdict { get; private set; }
    public int? LastQualityScore { get; private set; }

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
        ApplyEvaluation(true, 5, FlashcardAnswerVerdict.ExactCorrect, DateTime.UtcNow);
    }

    public void MarkWrong()
    {
        ApplyEvaluation(false, 0, FlashcardAnswerVerdict.Incorrect, DateTime.UtcNow);
    }

    public void ApplyReviewResult(bool isCorrect, DateTime? reviewedAt = null)
    {
        ApplyEvaluation(
            isCorrect,
            isCorrect ? 5 : 0,
            isCorrect ? FlashcardAnswerVerdict.ExactCorrect : FlashcardAnswerVerdict.Incorrect,
            reviewedAt);
    }

    public void ApplyReviewQuality(int qualityScore, DateTime? reviewedAt = null)
    {
        ApplyEvaluation(
            qualityScore >= 3,
            qualityScore,
            qualityScore >= 3 ? FlashcardAnswerVerdict.ExactCorrect : FlashcardAnswerVerdict.Incorrect,
            reviewedAt);
    }

    public void ApplyEvaluation(
        bool acceptedAsCorrect,
        int qualityScore,
        FlashcardAnswerVerdict verdict,
        DateTime? reviewedAt = null)
    {
        EnsureNotDeleted();

        if (qualityScore is < 0 or > 5)
        {
            throw new ArgumentException("Quality score must be between 0 and 5.", nameof(qualityScore));
        }

        DateTime reviewTime = NormalizeReviewTime(reviewedAt);
        ReviewCount += 1;
        LastReviewedAt = reviewTime;
        LastEvaluationVerdict = verdict;
        LastQualityScore = qualityScore;
        UpdatedAt = reviewTime;

        if (acceptedAsCorrect)
        {
            CorrectCount += 1;
            ConsecutiveCorrectCount += 1;
            ConsecutiveWrongCount = 0;
            LastReviewOutcome = FlashcardReviewOutcome.Correct;
            NextReviewAt = CalculateNextReviewAtAfterSuccess(reviewTime, qualityScore);
            return;
        }

        WrongCount += 1;
        LapseCount += 1;
        ConsecutiveCorrectCount = 0;
        ConsecutiveWrongCount += 1;
        LastReviewOutcome = FlashcardReviewOutcome.Wrong;
        NextReviewAt = CalculateNextReviewAtAfterLapse(reviewTime, qualityScore);
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
        LastReviewOutcome = null;
        LastEvaluationVerdict = null;
        LastQualityScore = null;
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

    private DateTime CalculateNextReviewAtAfterSuccess(DateTime reviewTime, int qualityScore)
    {
        if (ConsecutiveCorrectCount == 1)
        {
            return reviewTime.AddHours(qualityScore >= 5 ? 18 : 12);
        }

        if (ConsecutiveCorrectCount == 2)
        {
            return reviewTime.AddDays(qualityScore >= 5 ? 4 : 2);
        }

        int intervalDays = Math.Min(45, Math.Max(3, ConsecutiveCorrectCount * (qualityScore >= 5 ? 3 : 2)));
        return reviewTime.AddDays(intervalDays);
    }

    private static DateTime CalculateNextReviewAtAfterLapse(DateTime reviewTime, int qualityScore)
    {
        if (qualityScore <= 1)
        {
            return reviewTime.AddMinutes(10);
        }

        return reviewTime.AddMinutes(30);
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
