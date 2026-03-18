using EducAIte.Domain.Enum;

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

    private StudentCourse()
    {
    }

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
        TouchUpdatedAt();
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
        TouchUpdatedAt();
    }

    /// <summary>
    /// Adds a grade to the enrollment or restores an archived grade for the same grading period.
    /// </summary>
    /// <param name="gradeType">The grading period.</param>
    /// <param name="gradeValue">The grade value.</param>
    /// <returns>The created or restored grade.</returns>
    public Grade AddGrade(GradeType gradeType, decimal gradeValue)
    {
        EnsureNotDeleted();
        EnsurePersisted();

        if (HasGradeType(gradeType))
        {
            throw new InvalidOperationException("An active grade for the selected grading period already exists.");
        }

        Grade? archivedGrade = Grades.FirstOrDefault(grade => grade.IsDeleted && grade.GradeType == gradeType);
        if (archivedGrade is not null)
        {
            archivedGrade.Restore();
            archivedGrade.UpdateValue(gradeValue);
            TouchUpdatedAt();
            return archivedGrade;
        }

        Grade grade = new(StudentCourseId, gradeType, gradeValue);
        grade.AssignStudentCourse(this);
        Grades.Add(grade);
        TouchUpdatedAt();
        return grade;
    }

    /// <summary>
    /// Updates the grade value for an existing grading period.
    /// </summary>
    /// <param name="gradeType">The grading period to update.</param>
    /// <param name="gradeValue">The new grade value.</param>
    /// <returns>The updated grade.</returns>
    public Grade UpdateGrade(GradeType gradeType, decimal gradeValue)
    {
        EnsureNotDeleted();

        Grade grade = GetRequiredActiveGrade(gradeType);
        grade.UpdateValue(gradeValue);
        TouchUpdatedAt();
        return grade;
    }

    /// <summary>
    /// Archives an active grade for the specified grading period.
    /// </summary>
    /// <param name="gradeType">The grading period to archive.</param>
    /// <returns><see langword="true"/> when a grade was archived; otherwise, <see langword="false"/>.</returns>
    public bool RemoveGrade(GradeType gradeType)
    {
        EnsureNotDeleted();

        Grade? grade = GetGrade(gradeType);
        if (grade is null)
        {
            return false;
        }

        grade.MarkDeleted();
        TouchUpdatedAt();
        return true;
    }

    /// <summary>
    /// Retrieves an active grade for the specified grading period.
    /// </summary>
    /// <param name="gradeType">The grading period to retrieve.</param>
    /// <returns>The active grade when found; otherwise, <see langword="null"/>.</returns>
    public Grade? GetGrade(GradeType gradeType)
    {
        return Grades.FirstOrDefault(grade => !grade.IsDeleted && grade.GradeType == gradeType);
    }

    /// <summary>
    /// Retrieves all active grades that match the requested grading periods.
    /// </summary>
    /// <param name="gradeTypes">The grading periods to retrieve.</param>
    /// <returns>A read-only list of matching grades.</returns>
    public IReadOnlyList<Grade> GetGradesByTypes(IReadOnlyCollection<GradeType> gradeTypes)
    {
        ArgumentNullException.ThrowIfNull(gradeTypes);

        if (gradeTypes.Count == 0)
        {
            return Array.Empty<Grade>();
        }

        HashSet<GradeType> requestedTypes = gradeTypes.ToHashSet();
        return Grades
            .Where(grade => !grade.IsDeleted && requestedTypes.Contains(grade.GradeType))
            .OrderBy(grade => grade.GradeType)
            .ToList();
    }

    /// <summary>
    /// Determines whether an active grade exists for the specified grading period.
    /// </summary>
    /// <param name="gradeType">The grading period to check.</param>
    /// <returns><see langword="true"/> when an active grade exists; otherwise, <see langword="false"/>.</returns>
    public bool HasGradeType(GradeType gradeType)
    {
        return GetGrade(gradeType) is not null;
    }

    /// <summary>
    /// Returns all active grades recorded for the enrollment.
    /// </summary>
    /// <returns>A read-only list of active grades.</returns>
    public IReadOnlyList<Grade> GetActiveGrades()
    {
        return Grades
            .Where(grade => !grade.IsDeleted)
            .OrderBy(grade => grade.GradeType)
            .ToList();
    }

    /// <summary>
    /// Calculates the average grade across the regular grading periods.
    /// </summary>
    /// <returns>The regular-term average when grades exist; otherwise, <see langword="null"/>.</returns>
    public decimal? GetRegularTermAverage()
    {
        GradeType[] regularTerms =
        {
            GradeType.PRELIM,
            GradeType.MIDTERM,
            GradeType.SEMIFINAL,
            GradeType.FINAL
        };

        List<Grade> regularGrades = Grades
            .Where(grade => !grade.IsDeleted && regularTerms.Contains(grade.GradeType))
            .ToList();

        if (regularGrades.Count == 0)
        {
            return null;
        }

        return decimal.Round(regularGrades.Average(grade => grade.GradeValue), 2, MidpointRounding.AwayFromZero);
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
        TouchUpdatedAt();
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
        TouchUpdatedAt();
    }

    private Grade GetRequiredActiveGrade(GradeType gradeType)
    {
        Grade? grade = GetGrade(gradeType);
        if (grade is not null)
        {
            return grade;
        }

        throw new KeyNotFoundException("The requested grading period has no active grade.");
    }

    private void EnsurePersisted()
    {
        if (StudentCourseId <= 0)
        {
            throw new InvalidOperationException("Cannot add a grade to a student course that has not been persisted.");
        }
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
        TouchUpdatedAt();
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

    private void TouchUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
