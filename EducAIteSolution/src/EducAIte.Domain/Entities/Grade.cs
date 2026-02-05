using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Entities;

public class Grade
{
    // Primary Key
    public long GradeId { get; set;}

    // Foreign Keys
    public long StudentCourseId {get; set;}
    public StudentCourse StudentCourse { get; set; } = null!;

    public decimal GradeValue {get; set;}

    public GradeType GradeType {get; set;}

    
}   