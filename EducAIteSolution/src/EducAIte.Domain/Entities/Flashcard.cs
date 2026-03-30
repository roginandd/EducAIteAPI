namespace EducAIte.Domain.Entities;

public class Flashcard
{
    private readonly List<FlashcardAcceptedAnswerAlias> _acceptedAnswerAliases = [];

    // Primary Key
    public long FlashcardId { get; private set; }

    public string Question { get; private set; } = string.Empty;

    public string Answer { get; private set; } = string.Empty;

    public string ConceptExplanation { get; private set; } = string.Empty;

    public string AnsweringGuidance { get; private set; } = string.Empty;

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

    public IReadOnlyCollection<FlashcardAcceptedAnswerAlias> AcceptedAnswerAliases => _acceptedAnswerAliases.AsReadOnly();

    private Flashcard() { }

    public Flashcard(
        string question,
        string answer,
        long noteId,
        string? conceptExplanation = null,
        string? answeringGuidance = null,
        IEnumerable<string>? acceptedAnswerAliases = null)
    {
        Question = NormalizeQuestion(question);
        Answer = NormalizeAnswer(answer);
        ConceptExplanation = NormalizeOptionalText(conceptExplanation, nameof(conceptExplanation), 4000);
        AnsweringGuidance = NormalizeOptionalText(answeringGuidance, nameof(answeringGuidance), 2000);
        NoteId = ValidatePositiveId(noteId, nameof(noteId));
        ReplaceAcceptedAnswerAliases(acceptedAnswerAliases);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(
        string question,
        string answer,
        string? conceptExplanation = null,
        string? answeringGuidance = null,
        IEnumerable<string>? acceptedAnswerAliases = null)
    {
        EnsureNotDeleted();
        Question = NormalizeQuestion(question);
        Answer = NormalizeAnswer(answer);
        ConceptExplanation = NormalizeOptionalText(conceptExplanation, nameof(conceptExplanation), 4000);
        AnsweringGuidance = NormalizeOptionalText(answeringGuidance, nameof(answeringGuidance), 2000);
        ReplaceAcceptedAnswerAliases(acceptedAnswerAliases);
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

    private void ReplaceAcceptedAnswerAliases(IEnumerable<string>? acceptedAnswerAliases)
    {
        _acceptedAnswerAliases.Clear();

        if (acceptedAnswerAliases is null)
        {
            return;
        }

        int order = 0;
        foreach (string alias in acceptedAnswerAliases
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Select(value => value.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _acceptedAnswerAliases.Add(new FlashcardAcceptedAnswerAlias(alias, order));
            order += 1;
        }
    }

    private static string NormalizeOptionalText(string? value, string paramName, int maxLength)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
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
