namespace EducAIte.Domain.Entities;

public class Flashcard
{
    // Primary Key
    public long FlashcardId { get; private set; }

    public string Question { get; private set; } = string.Empty;

    public string Answer { get; private set; } = string.Empty;

    public long CourseId { get; private set; }
    public Course Course { get; private set; } = null!;


    // Foreign Key
    public long? NoteId { get; private set; }
    public Note? Note { get; set; }

    public long? DocumentId { get; private set; }
    public Document? Document { get; set; }
    
    // Additional Properties
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Flashcard() { }

    public Flashcard(string question, string answer, long courseId, long? noteId, long? documentId)
    {
        Question = NormalizeQuestion(question);
        Answer = NormalizeAnswer(answer);
        CourseId = ValidatePositiveId(courseId, nameof(courseId));
        NoteId = ValidateOptionalId(noteId, nameof(noteId));
        DocumentId = ValidateOptionalId(documentId, nameof(documentId));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string question, string answer)
    {
        EnsureNotDeleted();
        Question = NormalizeQuestion(question);
        Answer = NormalizeAnswer(answer);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachToNote(long noteId)
    {
        EnsureNotDeleted();
        NoteId = ValidatePositiveId(noteId, nameof(noteId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void DetachFromNote()
    {
        EnsureNotDeleted();
        NoteId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachToDocument(long documentId)
    {
        EnsureNotDeleted();
        DocumentId = ValidatePositiveId(documentId, nameof(documentId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void DetachFromDocument()
    {
        EnsureNotDeleted();
        DocumentId = null;
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
            throw new InvalidOperationException("Cannot modify a deleted flashcard.");
        }
    }

    private static string NormalizeQuestion(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question cannot be empty.", nameof(question));
        }

        string normalized = question.Trim();
        if (normalized.Length > 1000)
        {
            throw new ArgumentException("Question cannot exceed 1000 characters.", nameof(question));
        }

        return normalized;
    }

    private static string NormalizeAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new ArgumentException("Answer cannot be empty.", nameof(answer));
        }

        string normalized = answer.Trim();
        if (normalized.Length > 2000)
        {
            throw new ArgumentException("Answer cannot exceed 2000 characters.", nameof(answer));
        }

        return normalized;
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }

    private static long? ValidateOptionalId(long? id, string paramName)
    {
        if (id.HasValue && id.Value <= 0)
        {
            throw new ArgumentException("Id must be greater than zero when provided.", paramName);
        }

        return id;
    }
}
