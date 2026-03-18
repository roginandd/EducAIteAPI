namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Data Transfer Object representing the public view of a StudyLoad.
/// Includes the associated courses and calculated unit totals.
/// </summary>
public record StudyLoadResponse
{
    /// <summary>
    /// The sqid representation of the study load identifier.
    /// </summary>
    public string Sqid { get; init; } = string.Empty;

    /// <summary>
    /// The unique surrogate primary key of the study load.
    /// </summary>
    public long StudyLoadId { get; init; }

    /// <summary>
    /// The sqid representation of the student identifier.
    /// </summary>
    public string StudentSqid { get; init; } = string.Empty;

    /// <summary>
    /// The student ID associated with this study load.
    /// </summary>
    public long StudentId { get; init; }

    /// <summary>
    /// The sqid representation of the study load file metadata.
    /// </summary>
    public string FileMetadataSqid { get; init; } = string.Empty;

    /// <summary>
    /// Metadata for the uploaded study load document.
    /// </summary>
    public FileMetadataResponse? FileMetadata { get; init; }

    /// <summary>
    /// The school year for this study load (e.g., 2023-2024).
    /// </summary>
    public string SchoolYearStart { get; init; } = string.Empty;
    public string SchoolYearEnd { get; init; } = string.Empty;

    /// <summary>
    /// The semester label for this study load.
    /// </summary>
    public string Semester { get; init; } = string.Empty;

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
    public string Sqid { get; init; } = string.Empty;
    public long StudyLoadId { get; init; }
    public string StudentSqid { get; init; } = string.Empty;
    public long StudentId { get; init; }
    public string FileMetadataSqid { get; init; } = string.Empty;
    public FileMetadataResponse? FileMetadata { get; init; }
    public string SchoolYearStart { get; init; } = string.Empty;
    public string SchoolYearEnd { get; init; } = string.Empty;
    public string Semester { get; init; } = string.Empty;
    public IEnumerable<CourseResponse> Courses { get; init; } = new List<CourseResponse>();
    public int TotalUnits { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}


