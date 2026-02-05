using EducAIte.Domain.ValueObjects;

namespace EducAIte.Domain.Entities;


public class StudentCourse
{
    // Primary Key
    public long StudentCourseId { get; set; }

    // Foreign Keys
    public long CourseId { get; set; }
    public long StudentId { get; set; }

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public Student Student { get; set; } = null!;


    
    // Additional Properties
    public long StudyLoadId { get; set; }
    public SchoolYear SchoolYear { get; set; }
    public int Semester { get; set; }
}