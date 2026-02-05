namespace EducAIte.Domain.Entities;


public class Course
{   
    // Primary Key
    public long CourseId { get; set;}

    // Additional Properties
    public string EDPCode { get; set; } = string.Empty;
    
    public string CourseName {get; set;} = string.Empty;

    public byte Units {get; set;} 

    public DateTime CreatedAt {get; set;}

    public DateTime UpdatedAt {get; set;}

    
}