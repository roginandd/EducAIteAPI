namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Represents file metadata returned to the client.
/// </summary>
public record FileMetadataResponse
{
    /// <summary>
    /// Gets the sqid representation of the file metadata identifier.
    /// </summary>
    public string Sqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the underlying file metadata identifier.
    /// </summary>
    public long FileMetadataId { get; init; }

    /// <summary>
    /// Gets the sqid representation of the owning student identifier.
    /// </summary>
    public string StudentSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the owning student identifier.
    /// </summary>
    public long StudentId { get; init; }

    /// <summary>
    /// Gets the original file name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string FileExtension { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file content type.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the storage key.
    /// </summary>
    public string StorageKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSizeInBytes { get; init; }

    /// <summary>
    /// Gets the upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; init; }

    /// <summary>
    /// Gets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
