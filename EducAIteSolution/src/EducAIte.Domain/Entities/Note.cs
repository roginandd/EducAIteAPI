namespace EducAIte.Domain.Entities;

public class Note
{   
    // Primary Key
    public long NoteId { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string NoteContent { get; set; } = string.Empty;

    // Foreign Key
    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
}