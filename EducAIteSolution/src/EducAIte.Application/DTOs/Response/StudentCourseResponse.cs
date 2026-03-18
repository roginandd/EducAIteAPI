namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Represents an enrollment returned to the API client.
/// </summary>
public record StudentCourseResponse
{
    /// <summary>
    /// Gets the sqid representation of the enrollment identifier.
    /// </summary>
    public required string Sqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sqid representation of the course identifier.
    /// </summary>
    public required string CourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sqid representation of the study load identifier.
    /// </summary>
    public required string StudyLoadSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the course EDP code.
    /// </summary>
    public required string EDPCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the course display name.
    /// </summary>
    public required string CourseName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the number of course units.
    /// </summary>
    public byte Units { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the enrollment was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the enrollment was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
