using EducAIte.Domain.Enum;
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
    public required int SchoolYearStart { get; set; }
    public required int SchoolYearEnd { get; set; }
    public required Semester Semester { get; set; }
    public ICollection<Course> Courses{ get; set; } = new HashSet<Course>();

    public int TotalUnits => Courses.Sum(course => course.Units);

    // Navigation Property
    public Student Student { get; set;} = null!;

    // Additional Properties
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
}