namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents a single course payload inside a bulk course creation request.
/// </summary>
public record CreateBulkCourseItemRequest
{
    /// <summary>
    /// Gets the course EDP code.
    /// </summary>
    public required string EDPCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the course display name.
    /// </summary>
    public required string CourseName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the course unit count.
    /// </summary>
    public byte Units { get; init; }
}
