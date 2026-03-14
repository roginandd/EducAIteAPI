namespace EducAIte.Domain.Entities;

public class Document
{
    // Primary Key
    public long DocumentId { get; private set; }

    // Business fields
    public string DocumentName { get; private set; } = string.Empty;

    // Foreign Key to Folder
    public long FolderId { get; private set; }
    public Folder Folder { get; set; } = null!;

    // Reference to actual file
    public long FileMetadataId { get; private set; }
    public FileMetadata FileMetadata { get; set; } = null!;

    // Navigation Property
    private readonly HashSet<Note> _notes = new();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    private readonly HashSet<Flashcard> _flashcards = new();
    public IReadOnlyCollection<Flashcard> Flashcards => _flashcards.AsReadOnly();

    // Soft delete flag
    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Required by EF Core
    private Document() { }

    // Domain constructor
    public Document(string documentName, long folderId, long fileMetadataId)
    {
        DocumentName = NormalizeDocumentName(documentName);
        FolderId = ValidateFolderId(folderId);
        FileMetadataId = ValidateFileMetadataId(fileMetadataId);

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string documentName, long folderId, long fileMetadataId)
    {
        EnsureNotDeleted();

        DocumentName = NormalizeDocumentName(documentName);
        FolderId = ValidateFolderId(folderId);
        FileMetadataId = ValidateFileMetadataId(fileMetadataId);

        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeletedWithChildren(IEnumerable<Note> notes, IEnumerable<Flashcard> flashcards)
    {
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(flashcards);

        foreach (Note note in notes)
        {
            note.MarkDeleted();
        }

        foreach (Flashcard flashcard in flashcards)
        {
            flashcard.MarkDeleted();
        }

        MarkDeleted();
    }

    public void AddNote(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        EnsureNotDeleted();
        EnsureNoteBelongsToDocument(note);

        note.AssignToDocument(this);
        _notes.Add(note);
    }

    public void RemoveNote(Note note)
    {
        if (note == null)
            return;

        _notes.Remove(note);
    }

    public void AddFlashcard(Flashcard flashcard)
    {
        ArgumentNullException.ThrowIfNull(flashcard);
        EnsureNotDeleted();
        EnsureFlashcardBelongsToDocument(flashcard);

        flashcard.AssignToDocument(this);
        _flashcards.Add(flashcard);
    }

    public void RemoveFlashcard(Flashcard flashcard)
    {
        if (flashcard == null)
            return;

        _flashcards.Remove(flashcard);
    }

    public void ReassignNote(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);
        EnsureNotDeleted();

        if (note.IsDeleted)
            throw new InvalidOperationException("Cannot associate a deleted note with a document.");

        if (note.Document is not null && !ReferenceEquals(note.Document, this))
        {
            note.Document.RemoveNote(note);
        }

        note.AssignToDocument(this);
        _notes.Add(note);
    }

    public void ReassignFlashcard(Flashcard flashcard)
    {
        ArgumentNullException.ThrowIfNull(flashcard);
        EnsureNotDeleted();

        if (flashcard.IsDeleted)
            throw new InvalidOperationException("Cannot associate a deleted flashcard with a document.");

        if (flashcard.Document is not null && !ReferenceEquals(flashcard.Document, this))
        {
            flashcard.Document.RemoveFlashcard(flashcard);
        }

        flashcard.AssignToDocument(this);
        _flashcards.Add(flashcard);
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted document.");
    }

    private void EnsureNoteBelongsToDocument(Note note)
    {
        if (note.IsDeleted)
            throw new InvalidOperationException("Cannot associate a deleted note with a document.");

        if (note.DocumentId != DocumentId)
            throw new InvalidOperationException("Note is associated with a different document.");
    }

    private void EnsureFlashcardBelongsToDocument(Flashcard flashcard)
    {
        if (flashcard.IsDeleted)
            throw new InvalidOperationException("Cannot associate a deleted flashcard with a document.");

        if (flashcard.DocumentId != DocumentId)
            throw new InvalidOperationException("Flashcard is associated with a different document.");
    }

    private static string NormalizeDocumentName(string documentName)
    {
        if (string.IsNullOrWhiteSpace(documentName))
            throw new ArgumentException("Document name is required.", nameof(documentName));

        return documentName.Trim();
    }

    private static long ValidateFolderId(long folderId)
    {
        if (folderId <= 0)
            throw new ArgumentException("FolderId must be greater than zero.", nameof(folderId));

        return folderId;
    }

    private static long ValidateFileMetadataId(long fileMetadataId)
    {
        if (fileMetadataId <= 0)
            throw new ArgumentException("FileMetadataId must be greater than zero.", nameof(fileMetadataId));

        return fileMetadataId;
    }
}
