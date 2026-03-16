namespace EducAIte.Domain.Entities;

public class Note
{   
    // Primary Key
    public long NoteId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NoteContent { get; private set; } = string.Empty;


    public decimal SequenceNumber { get; private set; } // Order within a document

    // Foreign Key
    public long DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;
    
     // Additional Properties
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt {get; private set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; private set;} = DateTime.UtcNow;

    private readonly HashSet<Flashcard> _flashcards = new();
    public IReadOnlyCollection<Flashcard> Flashcards => _flashcards.AsReadOnly();


    // Constructor for creating a new note
    private Note () { }
    

    // Domain constructor
    public Note (string name, string noteContent, long documentId, decimal sequenceNumber)
    {
        Name = NormalizeNoteName(name);
        NoteContent = NormalizeNoteContent(noteContent);
        DocumentId = ValidateDocumentId(documentId);
        SequenceNumber = ValidateSequenceNumber(sequenceNumber);

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string noteContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted note.");

        Name = NormalizeNoteName(name);
        NoteContent = NormalizeNoteContent(noteContent);

        UpdatedAt = DateTime.UtcNow;

    }

    public void Rename(string name)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted note.");

        Name = NormalizeNoteName(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string noteContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted note.");

        NoteContent = NormalizeNoteContent(noteContent);
        UpdatedAt = DateTime.UtcNow;
    }

    internal void AssignToDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted note.");

        if (document.IsDeleted)
            throw new InvalidOperationException("Cannot associate a note with a deleted document.");

        DocumentId = ValidateDocumentId(document.DocumentId);
        Document = document;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSequenceNumber(decimal newSequenceNumber)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted note.");

        SequenceNumber = ValidateSequenceNumber(newSequenceNumber);

        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeletedWithChildren()
    {
        foreach (Flashcard flashcard in _flashcards)
        {
            flashcard.MarkDeleted();
        }

        MarkDeleted();
    }

    public void AddFlashcard(Flashcard flashcard)
    {
        ArgumentNullException.ThrowIfNull(flashcard);

        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted note.");
        }

        EnsureFlashcardBelongsToNote(flashcard);
        flashcard.AssignToNote(this);
        _flashcards.Add(flashcard);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddFlashcards(IEnumerable<Flashcard> flashcards)
    {
        ArgumentNullException.ThrowIfNull(flashcards);

        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted note.");
        }

        List<Flashcard> items = flashcards.ToList();
        if (items.Count == 0)
        {
            return;
        }

        foreach (Flashcard flashcard in items)
        {
            ArgumentNullException.ThrowIfNull(flashcard);
            EnsureFlashcardBelongsToNote(flashcard);
        }

        foreach (Flashcard flashcard in items)
        {
            flashcard.AssignToNote(this);
            _flashcards.Add(flashcard);
        }

        UpdatedAt = DateTime.UtcNow;
    }


    public void RemoveFlashcard(Flashcard flashcard)
    {
        if (flashcard is null)
        {
            return;
        }

        _flashcards.Remove(flashcard);
    }

    public void ReassignFlashcard(Flashcard flashcard)
    {
        ArgumentNullException.ThrowIfNull(flashcard);

        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted note.");
        }

        if (flashcard.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate a deleted flashcard with a note.");
        }

        if (flashcard.Note is not null && !ReferenceEquals(flashcard.Note, this))
        {
            flashcard.Note.RemoveFlashcard(flashcard);
        }

        flashcard.AssignToNote(this);
        _flashcards.Add(flashcard);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeNoteName(string name)
    {
        bool isNameEmpty = string.IsNullOrWhiteSpace(name);

        if (isNameEmpty)
            throw new ArgumentException("Note name cannot be empty.", nameof(name));

        string normalizedName = name.Trim();
        if (normalizedName.Length > 200)
            throw new ArgumentException("Note name cannot exceed 200 characters.", nameof(name));

        return normalizedName;
    }

    private static string NormalizeNoteContent(string content)
    {
        bool isContentEmpty = string.IsNullOrWhiteSpace(content);

        if (isContentEmpty)
            throw new ArgumentException("Note content cannot be empty.", nameof(content));

        return content.Trim();
    }

    private static long ValidateDocumentId(long documentId)
    {
        if (documentId <= 0)
            throw new ArgumentException("DocumentId must be greater than zero.", nameof(documentId));

        return documentId;
    }

    private static decimal ValidateSequenceNumber(decimal sequenceNumber)
    {
        if (sequenceNumber < 0)
            throw new ArgumentException("Sequence number cannot be negative.", nameof(sequenceNumber));
        
        return sequenceNumber;
    }

    private void EnsureFlashcardBelongsToNote(Flashcard flashcard)
    {
        if (flashcard.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate a deleted flashcard with a note.");
        }

        if (flashcard.NoteId != NoteId)
        {
            throw new InvalidOperationException("Flashcard is associated with a different note.");
        }
    }
}
