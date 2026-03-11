namespace EducAIte.Domain.Entities;

public class Document
{
    // Primary Key
    public long DocumentId { get; private set; }

    // Public identifier safe for external exposure
    public Guid ExternalId { get; private set; }

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
    public IReadOnlyCollection<Note> Notes => _notes;

    // Soft delete flag
    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Required by EF Core
    private Document() { }

    // Domain constructor
    public Document(string documentName, long folderId, long fileMetadataId)
    {
        ExternalId = Guid.NewGuid();
        DocumentName = NormalizeDocumentName(documentName);
        FolderId = ValidateFolderId(folderId);
        FileMetadataId = ValidateFileMetadataId(fileMetadataId);

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string documentName, long folderId, long fileMetadataId)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted document.");

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

    public void AddNote(Note note)
    {
        if (note == null)
            throw new ArgumentNullException(nameof(note));

        _notes.Add(note);
    }

    public void RemoveNote(Note note)
    {
        if (note == null)
            return;

        _notes.Remove(note);
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
