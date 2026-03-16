using EducAIte.Domain.ValueObjects;
using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Response;

public record StudyLoadResponse
{
    public string StudyLoadSqid { get; init; } = string.Empty;

    public string StudentSqid { get; init; } = string.Empty;

    public string FileMetadataSqid { get; init; } = string.Empty;

    public FileMetadataResponse FileMetadata { get; init; } = new();

    public SchoolYear SchoolYear { get; init; } = new SchoolYear(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 1);

    public Semester Semester { get; init; }

    public ICollection<CourseResponse> Courses { get; init; } = new HashSet<CourseResponse>();

    public int TotalUnits { get; init; }

    public StudentResponse Student { get; init; } = new(0, string.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, default);

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}


