namespace EducAIte.Application.DTOs.Response;

public record DocumentResponse
{
    public long DocumentId { get; init; }

    public Guid ExternalId { get; init; }

    public string DocumentName { get; init; } = string.Empty;

    public long FolderId { get; init; }

    public long FileMetadataId { get; init; }

    public long StudentId { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
