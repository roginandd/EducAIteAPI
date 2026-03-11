namespace EducAIte.Application.DTOs.Response;

public record DocumentResponse
{
    public string Sqid { get; init; } = string.Empty;

    public string DocumentName { get; init; } = string.Empty;

    public string FolderSqid { get; init; } = string.Empty;

    public string FileMetadataSqid { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
