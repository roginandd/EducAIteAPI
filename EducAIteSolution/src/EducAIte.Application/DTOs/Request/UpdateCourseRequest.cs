namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Data Transfer Object representing a request to update an existing Course's attributes.
/// Note: EDPCode is omitted to prevent accidental modification of natural keys.
/// </summary>
public record UpdateCourseRequest
{
    /// <summary>
    /// The updated name of the course.
    /// </summary>
    public string CourseName { get; init; } = string.Empty;

    /// <summary>
    /// The updated EDP code of the course.
    /// </summary>
    public string EDPCode { get; init; } = string.Empty;


    /// <summary>
    /// The updated academic credit value.
    /// </summary>
    public byte Units { get; init; }
}