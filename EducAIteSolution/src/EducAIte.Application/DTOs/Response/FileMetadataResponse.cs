namespace EducAIte.Application.DTOs.Response;

public record FileMetadataResponse
{
    public long FileMetaDataId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string FileExtension { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public string StorageKey { get; init; } = string.Empty;

    public long FileSizeInBytes { get; init; }

    public DateTime UploadedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
