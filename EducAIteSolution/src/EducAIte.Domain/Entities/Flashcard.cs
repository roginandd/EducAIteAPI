namespace EducAIte.Domain.Entities;

public class Flashcard
{
    // Primary Key
    public long FlashcardId { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public long CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Foreign Key
    public long NoteId { get; set; }
    public Note? Note { get; set; }

    public long? DocumentId { get; set; }
    public Document? Document { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set;} 

    
}