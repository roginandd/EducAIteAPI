namespace EducAIte.Application.DTOs.Response;

public record FolderContentItemResponse
{
    public string Sqid { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
