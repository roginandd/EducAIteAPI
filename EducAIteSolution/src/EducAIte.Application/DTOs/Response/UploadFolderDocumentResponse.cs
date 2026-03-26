namespace EducAIte.Application.DTOs.Response;

public record UploadFolderDocumentResponse
{
    public DocumentResponse Document { get; init; } = new();

    public FileMetadataResponse FileMetadata { get; init; } = new();
}
