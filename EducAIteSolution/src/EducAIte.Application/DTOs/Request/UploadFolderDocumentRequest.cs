namespace EducAIte.Application.DTOs.Request;

public record UploadFolderDocumentRequest
{
    public string? DocumentName { get; init; }

    public required IFormFile File { get; init; }
}
