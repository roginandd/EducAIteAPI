namespace EducAIte.Domain.Entities;

public class Flashcard
{
    // Primary Key
    public long FlashcardId { get; private set; }

    public string Question { get; private set; } = string.Empty;

    public string Answer { get; private set; } = string.Empty;

    // Foreign Key
    public long NoteId { get; private set; }
    public Note Note { get; private set; } = null!;
    
    // Additional Properties
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // Navigational Properties
    private readonly HashSet<StudentFlashcard> _studentFlashcards = new();
    public IReadOnlyCollection<StudentFlashcard> StudentFlashcards => _studentFlashcards.AsReadOnly();

    private Flashcard() { }

    public Flashcard(string question, string answer, long noteId)
    {
        Question = NormalizeQuestion(question);
        Answer = NormalizeAnswer(answer);
        NoteId = ValidatePositiveId(noteId, nameof(noteId));
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

    internal void AssignToNote(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        EnsureNotDeleted();

        if (note.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate a flashcard with a deleted note.");
        }

        NoteId = ValidatePositiveId(note.NoteId, nameof(note.NoteId));
        Note = note;
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

    public void MarkDeletedWithProgress(IEnumerable<StudentFlashcard> studentFlashcards)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcards);

        foreach (StudentFlashcard studentFlashcard in studentFlashcards)
        {
            studentFlashcard.MarkDeleted();
        }

        MarkDeleted();
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

}
