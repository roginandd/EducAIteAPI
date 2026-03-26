namespace EducAIte.Application.DTOs.Response;

public record FolderSearchResponse
{
    public string FolderSqid { get; init; } = string.Empty;

    public string Query { get; init; } = string.Empty;

    public IReadOnlyList<FolderSearchResultResponse> Results { get; init; } = [];
}
