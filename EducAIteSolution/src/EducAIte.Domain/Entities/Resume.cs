namespace EducAIte.Domain.Entities;

public class Resume
{
    public long ResumeId { get; set; }

    public long StudentId { get; set; }

    public string ResumeKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Navigation Properties
    public Student Student { get; set; } = null!;
}