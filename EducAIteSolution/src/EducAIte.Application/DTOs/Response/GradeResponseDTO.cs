using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Represents a grade returned to the API client.
/// </summary>
public record GradeResponseDTO
{
    /// <summary>
    /// Gets the sqid representation of the grade identifier.
    /// </summary>
    public required string Sqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sqid representation of the owning student course identifier.
    /// </summary>
    public required string StudentCourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the grading period.
    /// </summary>
    public required GradeType GradeType { get; init; }

    /// <summary>
    /// Gets the numeric grade value.
    /// </summary>
    public required decimal GradeValue { get; init; }

    /// <summary>
    /// Gets a value indicating whether the grade is passing.
    /// </summary>
    public required bool IsPassing { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the grade was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the grade was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
