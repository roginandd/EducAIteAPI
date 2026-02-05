namespace EducAIte.Domain.Entities;


public class FileMetadata
{
    // Primary Key (Surrogate Key)
    public long FileMetaDataId { get; set; }
    public string FileName { get; set; } = string.Empty; 
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty; // The path in S3/Firebase
    public long FileSizeInBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Audit: Who uploaded it?
    public long StudentId { get; set; }
    public Student Student { get; set; } = null!;
}