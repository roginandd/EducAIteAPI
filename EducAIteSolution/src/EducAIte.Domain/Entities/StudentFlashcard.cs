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

    // Additional Properties
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StudentFlashcard() { }

    public StudentFlashcard(long studentId, long flashcardId)
    {
        StudentId = ValidatePositiveId(studentId, nameof(studentId));
        FlashcardId = ValidatePositiveId(flashcardId, nameof(flashcardId));
        CorrectCount = 0;
        WrongCount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCorrect()
    {
        EnsureNotDeleted();
        CorrectCount += 1;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkWrong()
    {
        EnsureNotDeleted();
        WrongCount += 1;
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
}
