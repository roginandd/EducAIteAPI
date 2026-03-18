using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Entities;

/// <summary>
/// Represents a recorded grade for a specific student course and grading period.
/// </summary>
public class Grade
{
    /// <summary>
    /// Gets the surrogate primary key of the grade.
    /// </summary>
    public long GradeId { get; private set; }

    /// <summary>
    /// Gets the owning student course identifier.
    /// </summary>
    public long StudentCourseId { get; private set; }

    /// <summary>
    /// Gets the owning student course aggregate.
    /// </summary>
    public StudentCourse StudentCourse { get; private set; } = null!;

    /// <summary>
    /// Gets the recorded grade value using the institutional 1.00 to 5.00 scale.
    /// </summary>
    public decimal GradeValue { get; private set; }

    /// <summary>
    /// Gets the grading period represented by this grade.
    /// </summary>
    public GradeType GradeType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the grade is archived.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the grade was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when the grade was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Grade()
    {
    }

    /// <summary>
    /// Initializes a new grade for a student course and grading period.
    /// </summary>
    /// <param name="studentCourseId">The owning student course identifier.</param>
    /// <param name="gradeType">The grading period.</param>
    /// <param name="gradeValue">The grade value.</param>
    public Grade(long studentCourseId, GradeType gradeType, decimal gradeValue)
    {
        StudentCourseId = ValidatePositiveId(studentCourseId, nameof(studentCourseId));
        GradeType = ValidateGradeType(gradeType);
        GradeValue = ValidateGradeValue(gradeValue);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the numeric grade value.
    /// </summary>
    /// <param name="newGradeValue">The new grade value.</param>
    public void UpdateValue(decimal newGradeValue)
    {
        EnsureNotDeleted();
        GradeValue = ValidateGradeValue(newGradeValue);
        TouchUpdatedAt();
    }

    /// <summary>
    /// Changes the grading period associated with the grade.
    /// </summary>
    /// <param name="newGradeType">The new grading period.</param>
    public void ChangeType(GradeType newGradeType)
    {
        EnsureNotDeleted();
        GradeType = ValidateGradeType(newGradeType);
        TouchUpdatedAt();
    }

    /// <summary>
    /// Associates the grade with a loaded student course aggregate.
    /// </summary>
    /// <param name="studentCourse">The owning student course.</param>
    public void AssignStudentCourse(StudentCourse studentCourse)
    {
        ArgumentNullException.ThrowIfNull(studentCourse);
        EnsureNotDeleted();

        if (studentCourse.IsDeleted)
        {
            throw new InvalidOperationException("Cannot associate a grade with a deleted student course.");
        }

        StudentCourseId = ValidatePositiveId(studentCourse.StudentCourseId, nameof(studentCourse.StudentCourseId));
        StudentCourse = studentCourse;
        TouchUpdatedAt();
    }

    /// <summary>
    /// Marks the grade as archived.
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

    /// <summary>
    /// Restores a previously archived grade.
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

    /// <summary>
    /// Determines whether the current grade value is passing.
    /// </summary>
    /// <returns><see langword="true"/> when the grade is passing; otherwise, <see langword="false"/>.</returns>
    public bool IsPassing()
    {
        return GradeValue <= 3.00m;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted grade.");
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

    private static GradeType ValidateGradeType(GradeType gradeType)
    {   
        bool isGradeTypeValid = System.Enum.IsDefined(typeof(GradeType), gradeType);

        if (!isGradeTypeValid)
        {
            throw new ArgumentException("Grade type is invalid.", nameof(gradeType));
        }

        return gradeType;
    }

    private static decimal ValidateGradeValue(decimal gradeValue)
    {
        decimal normalizedValue = decimal.Round(gradeValue, 2, MidpointRounding.AwayFromZero);
        if (normalizedValue < 1.00m || normalizedValue > 5.00m)
        {
            throw new ArgumentException("Grade value must be between 1.00 and 5.00.", nameof(gradeValue));
        }

        return normalizedValue;
    }

    private void TouchUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
