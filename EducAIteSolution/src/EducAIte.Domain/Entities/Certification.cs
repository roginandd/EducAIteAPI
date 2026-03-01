namespace EducAIte.Domain.Entities;

public class Certification
{
    public long CertificationId { get; set; }

    public long StudentId { get; set; }

    public string CertificationKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Navigation Properties
    public Student Student { get; set; } = null!;
}