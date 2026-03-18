namespace EducAIte.Domain.Entities;

/// <summary>
/// Represents a student's enrollment in a course for a specific study load.
/// </summary>
public class StudentCourse
{
    /// <summary>
    /// Gets the surrogate primary key of the enrollment.
    /// </summary>
    public long StudentCourseId { get; private set; }

    /// <summary>
    /// Gets the linked course identifier.
    /// </summary>
    public long CourseId { get; private set; }

    /// <summary>
    /// Gets the linked study load identifier.
    /// </summary>
    public long StudyLoadId { get; private set; }

    /// <summary>
    /// Gets the enrolled course.
    /// </summary>
    public Course Course { get; private set; } = null!;

    /// <summary>
    /// Gets the grades recorded against this enrollment.
    /// </summary>
    public List<Grade> Grades { get; private set; } = new();

    /// <summary>
    /// Gets the owning study load.
    /// </summary>
    public StudyLoad StudyLoad { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the enrollment is archived.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the enrollment was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when the enrollment was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StudentCourse() { }

    /// <summary>
    /// Initializes a new enrollment for a course and study load.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <param name="studyLoadId">The study load identifier.</param>
    public StudentCourse(long courseId, long studyLoadId)
    {
        CourseId = ValidatePositiveId(courseId, nameof(courseId));
        StudyLoadId = ValidatePositiveId(studyLoadId, nameof(studyLoadId));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associates the enrollment with a loaded course entity.
    /// </summary>
    /// <param name="course">The course entity.</param>
    public void AssignCourse(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);
        EnsureNotDeleted();

        if (course.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate an enrollment with a deleted course.");
        }

        CourseId = ValidatePositiveId(course.CourseId, nameof(course.CourseId));
        Course = course;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associates the enrollment with a loaded study load entity.
    /// </summary>
    /// <param name="studyLoad">The study load entity.</param>
    public void AssignStudyLoad(StudyLoad studyLoad)
    {
        ArgumentNullException.ThrowIfNull(studyLoad);
        EnsureNotDeleted();

        if (studyLoad.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate an enrollment with a deleted study load.");
        }

        StudyLoadId = ValidatePositiveId(studyLoad.StudyLoadId, nameof(studyLoad.StudyLoadId));
        StudyLoad = studyLoad;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the enrollment as deleted without removing the database row.
    /// </summary>
    public void MarkDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a previously archived enrollment.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted student course.");
        }
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }
}
