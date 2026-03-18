using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents the data required to update a grade for a student course.
/// </summary>
public record UpdateGradeDTO
{
    /// <summary>
    /// Gets the sqid representation of the owning student course identifier.
    /// </summary>
    public required string StudentCourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the grading period to update.
    /// </summary>
    public required GradeType GradeType { get; init; }

    /// <summary>
    /// Gets the new numeric grade value.
    /// </summary>
    public required decimal GradeValue { get; init; }
}
