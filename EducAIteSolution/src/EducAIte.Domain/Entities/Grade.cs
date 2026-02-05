using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Entities;

public class Grade
{
    // Primary Key
    public long GradeId { get; set;}

    // Foreign Keys
    public long StudentId {get; set;}

    public long CourseId {get; set;}

    public decimal GradeValue {get; set;}

    public GradeType GradeType {get; set;}

    // Navigation Property
    public Student Student { get; set;} = null!;
    
}