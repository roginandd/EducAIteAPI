namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Data Transfer Object for creating a new StudyLoad.
/// </summary>
public record StudyLoadCreateRequest
{
    /// <summary>
    /// The student ID associated with this study load.
    /// </summary>
    public required string StudentSqid { get; init; }

    /// <summary>
    /// The starting year of the school year.
    /// </summary>
    public required int SchoolYearStart { get; init; }

    /// <summary>
    /// The ending year of the school year.
    /// </summary>
    public required int SchoolYearEnd { get; init; }
    public required int Semester { get; init; }
    public required IFormFile StudyLoadDocument { get; init; }
}

/// <summary>
/// Data Transfer Object for updating an existing StudyLoad.
/// </summary>
public record StudyLoadUpdateRequest
{
    /// <summary>
    /// The starting year of the school year.
    /// </summary>
    public int? SchoolYearStart { get; init; }

    /// <summary>
    /// The ending year of the school year.
    /// </summary>
    public int? SchoolYearEnd { get; init; }

    /// <summary>
    /// The semester.
    /// </summary>
    public int? Semester { get; init; }
}
