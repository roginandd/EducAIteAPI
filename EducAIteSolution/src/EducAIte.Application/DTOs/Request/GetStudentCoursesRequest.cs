/// <summary>
/// Represents optional filters for retrieving the authenticated student's course enrollments.
/// </summary>
public record GetStudentCoursesRequest
{
    /// <summary>
    /// Gets the semester value to filter by.
    /// </summary>
    public int? Semester { get; init; }

    /// <summary>
    /// Gets the school year start value to filter by.
    /// </summary>
    public int? SchoolYearStart { get; init; }

    /// <summary>
    /// Gets the school year end value to filter by.
    /// </summary>
    public int? SchoolYearEnd { get; init; }
}
