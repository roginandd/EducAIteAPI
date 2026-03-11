namespace EducAIte.Application.DTOs.Request;

public record UpdateDocumentRequest
{
    public required string DocumentName { get; init; } = string.Empty;

    public long FolderId { get; init; }

    public long FileMetadataId { get; init; }
}
