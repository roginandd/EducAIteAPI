namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Data Transfer Object representing a request to create a new Course.
/// Immutability ensures that the message cannot be modified once sent.
/// </summary>
public record CreateCourseRequest
{
    /// <summary>
    /// The unique electronic data processing code (Natural Key).
    /// Once created, this is generally immutable due to historical relationships.
    /// </summary>
    public required string EDPCode { get; init; } = string.Empty;

    /// <summary>
    /// The descriptive name of the course.
    /// </summary>
    public required string CourseName { get; init; } = string.Empty;

    /// <summary>
    /// The academic credit value assigned to the course.
    /// </summary>
    public byte Units { get; init; }
}