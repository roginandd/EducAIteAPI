namespace EducAIte.Application.DTOs.Request;

public record UpdateFolderRequest
{
    public required string FolderKey { get; init; } = string.Empty;

    public required string Name { get; init; } = string.Empty;

    public int SchoolYearStart { get; init; }

    public int SchoolYearEnd { get; init; }

    public byte Semester { get; init; }

    public required string StudentCourseSqid { get; init; } = string.Empty;

    public string? ParentFolderSqid { get; init; }
}
