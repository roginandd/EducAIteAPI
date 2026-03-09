namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Data Transfer Object representing the system's public view of a Course.
/// Immutability ensures consistent state presentation to the client.
/// </summary>
public record CourseResponse
{
    /// <summary>
    /// The unique surrogate primary key.
    /// Used for internal relationship identification.
    /// </summary>
    public long CourseId { get; init; }

    /// <summary>
    /// The unique electronic data processing code.
    /// Used as a natural identifier for external lookups.
    /// </summary>
    public string EDPCode { get; init; } = string.Empty;

    /// <summary>
    /// The descriptive name of the course.
    /// </summary>
    public string CourseName { get; init; } = string.Empty;

    /// <summary>
    /// The academic credit value assigned to the course.
    /// </summary>
    public byte Units { get; init; }

    /// <summary>
    /// The timestamp indicating when the course record was originally created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}