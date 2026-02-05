using EducAIte.Domain.ValueObjects;

namespace EducAIte.Domain.Entities;

public class StudyLoad
{

    // Primary Key (Surrogate Key)
    public long StudyLoadId { get; set; }


    // Foreign Key 
    public long StudentId { get; set;}

    public long FileMetadataId { get; set; }
    
    public FileMetadata FileMetadata { get; set; } = null!;

    // Properties
    public SchoolYear SchoolYear{ get; set; }

    public ICollection<Course> Courses{ get; set; } = new HashSet<Course>();

    public int TotalUnits => Courses.Sum(course => course.Units);

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    // Navigation Property
    public Student Student { get; set;} = null!;
}