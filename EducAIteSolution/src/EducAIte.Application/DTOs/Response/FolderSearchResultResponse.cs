namespace EducAIte.Application.DTOs.Response;

public record FolderSearchResultResponse
{
    public string Sqid { get; init; } = string.Empty;

    public string ItemType { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string LocationDisplayPath { get; init; } = string.Empty;
}
