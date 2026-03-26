namespace EducAIte.Application.DTOs.Response;

public record FolderContentsResponse
{
    public FolderResponse Folder { get; init; } = new();

    public IReadOnlyList<FolderContentItemResponse> SubFolders { get; init; } = [];

    public IReadOnlyList<FolderContentItemResponse> Documents { get; init; } = [];

    public IReadOnlyList<FolderContentItemResponse> Notes { get; init; } = [];
}
