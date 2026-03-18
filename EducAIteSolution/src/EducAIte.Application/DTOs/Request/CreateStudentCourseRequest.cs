namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents the data required to enroll a student into a course for a study load.
/// </summary>
public record CreateStudentCourseRequest
{
    /// <summary>
    /// Gets the course identifier to enroll in.
    /// </summary>
    public required long CourseId { get; init; }

    /// <summary>
    /// Gets the study load identifier that will own the enrollment.
    /// </summary>
    public required long StudyLoadId { get; init; }
}
