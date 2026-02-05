namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class Student
{
    // Primary Key (Surrogate Key)
    public long StudentId {get; set;} 
    
    public string StudentIdNumber {get; set;} = string.Empty;

    public StudentPrograms Program {get; set;}

    public DateTime BirthDate {get; set;}

    public int Semester {get; set;}

    public string FirstName {get; set;} = string.Empty;

    public string LastName {get; set;} = string.Empty;

    public string MiddleName {get; set;} = string.Empty;

    public string PasswordHash {get; set;} = string.Empty;

    public string Email {get; set;} = string.Empty;

    public string PhoneNumber {get; set; } = string.Empty;

    public DateTime CreatedAt {get; set;}

    public DateTime UpdatedAt {get; set;}

    // Navigation Properties
    public ICollection<StudyLoad> StudyLoads { get; set;} = new HashSet<StudyLoad>();

    public ICollection<Folder> Folders { get; set; } = new HashSet<Folder>();

    public ICollection<FileMetadata> UploadedFiles { get; set; } = new HashSet<FileMetadata>();

    public ICollection<StudentFlashcard> Flashcards { get; set; } = new HashSet<StudentFlashcard>();

    public ICollection<StudentCourse> EnrolledCourses { get; set; } = new HashSet<StudentCourse>();
    
}