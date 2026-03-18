using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents the data required to create a grade for a student course.
/// </summary>
public record CreateGradeDTO
{
    /// <summary>
    /// Gets the sqid representation of the owning student course identifier.
    /// </summary>
    public required string StudentCourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the grading period to create.
    /// </summary>
    public required GradeType GradeType { get; init; }

    /// <summary>
    /// Gets the numeric grade value to record.
    /// </summary>
    public required decimal GradeValue { get; init; }
}
