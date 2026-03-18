using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents a grade query that filters a student course by grading periods.
/// </summary>
public record GetGradesByTypeDTO
{
    /// <summary>
    /// Gets the sqid representation of the owning student course identifier.
    /// </summary>
    public required string StudentCourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the grading periods to include in the result.
    /// </summary>
    public IReadOnlyCollection<GradeType> GradeTypes { get; init; } = Array.Empty<GradeType>();
}
