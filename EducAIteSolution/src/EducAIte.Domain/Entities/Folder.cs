using EducAIte.Domain.ValueObjects;

namespace EducAIte.Domain.Entities;


public class Folder
{
    public long FolderId { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    public long StudentId {get; set;}

    public Student Student { get; set; } = null!;

    public SchoolYear SchoolYear { get; set; }

    public byte Semester { get; set; }

    public string FolderKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public long? CourseId { get; set; } // Link to the actual Course entity
    public Course? Course { get; set; }

    public long? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }

    public ICollection<Folder> SubFolders { get; set; } = new HashSet<Folder>();
    public ICollection<Document> Documents { get; set; } = new HashSet<Document>();

    public ICollection<StudentFlashcard> StudentFlashcards { get; set; } = new HashSet<StudentFlashcard>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}