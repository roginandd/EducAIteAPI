namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Data Transfer Object representing the public view of a StudyLoad.
/// Includes the associated courses and calculated unit totals.
/// </summary>
public record StudyLoadResponse
{
    /// <summary>
    /// The unique surrogate primary key of the study load.
    /// </summary>
    public long StudyLoadId { get; init; }

    /// <summary>
    /// The student ID associated with this study load.
    /// </summary>
    public long StudentId { get; init; }

    /// <summary>
    /// The school year for this study load (e.g., 2023-2024).
    /// </summary>
    public string SchoolYear { get; init; } = string.Empty;

    /// <summary>
    /// Collection of courses enrolled in this study load.
    /// </summary>
    public IEnumerable<CourseResponse> Courses { get; init; } = new List<CourseResponse>();

    /// <summary>
    /// The total number of units across all courses in this study load.
    /// </summary>
    public int TotalUnits { get; init; }

    /// <summary>
    /// The timestamp when this study load was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The timestamp when this study load was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Data Transfer Object for detailed study load information.
/// Used when retrieving studyload data in application layer.
/// </summary>
public record StudyLoadDto
{
    public long StudyLoadId { get; init; }
    public long StudentId { get; init; }
    public string SchoolYear { get; init; } = string.Empty;
    public IEnumerable<CourseResponse> Courses { get; init; } = new List<CourseResponse>();
    public int TotalUnits { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}


