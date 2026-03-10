namespace EducAIte.Domain.Entities;


public class Document
{

    // Primary Key
    public long DocumentId { get; set; }
    
    public Guid ExternalId { get; set; } = Guid.NewGuid();
   
    public string DocumentName { get; set;} = string.Empty;

    // Foreign Key to the Folder entity
    public long FolderId { get; set; }
    public Folder Folder { get; set; } = null!;

    // Reference to the actual file
    public long FileMetadataId { get; set; }
    public FileMetadata FileMetadata { get; set; } = null!;

    // Navigation Property for Notes
    public ICollection<Note> Notes { get; set; } = new HashSet<Note>();

    // Aditional Properties
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
}