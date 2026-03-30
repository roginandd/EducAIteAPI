namespace EducAIte.Application.DTOs.Response;

public record FolderResponse
{
    public string Sqid { get; init; } = string.Empty;

    public string StudentSqid { get; init; } = string.Empty;

    public string FolderKey { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int SchoolYearStart { get; init; }

    public int SchoolYearEnd { get; init; }

    public string SchoolYearDisplayName { get; init; } = string.Empty;

    public byte Semester { get; init; }

    public string StudentCourseSqid { get; init; } = string.Empty;

    public string? ParentFolderSqid { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
