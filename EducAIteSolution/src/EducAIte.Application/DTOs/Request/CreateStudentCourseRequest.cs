namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents the data required to enroll a student into a course for a study load.
/// </summary>
public record CreateStudentCourseRequest
{
    /// <summary>
    /// Gets the sqid representation of the course identifier to enroll in.
    /// </summary>
    public required string CourseSqid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the sqid representation of the study load identifier that will own the enrollment.
    /// </summary>
    public required string StudyLoadSqid { get; init; } = string.Empty;
}
