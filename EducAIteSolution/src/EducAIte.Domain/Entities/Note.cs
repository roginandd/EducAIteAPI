namespace EducAIte.Domain.Entities;

public class Note
{   
    // Primary Key
    public long NoteId { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string NoteContent { get; set; } = string.Empty;


    public long SequenceNumber { get; set; } // To maintain the order of notes within a document

    // Foreign Key
    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    

    // Navigation Property for Flashcards
    public ICollection<Flashcard> Flashcards { get; set; } = new HashSet<Flashcard>();
    
    // Additional Properties
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
    
}