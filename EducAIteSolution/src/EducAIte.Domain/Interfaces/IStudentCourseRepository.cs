using EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

namespace EducAIte.Domain.Interfaces;

/// <summary>
/// Defines persistence operations for student course enrollments.
/// </summary>
public interface IStudentCourseRepository
{
    /// <summary>
    /// Retrieves an active enrollment by its identifier.
    /// </summary>
    Task<StudentCourse?> GetByIdAsync(long studentCourseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an active enrollment owned by a specific student.
    /// </summary>
    Task<StudentCourse?> GetByIdAndStudentIdAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active enrollments for a student.
    /// </summary>
    Task<IReadOnlyList<StudentCourse>> GetAllByStudentIdAsync(
        long studentId,
        Semester? semester = null,
        int? schoolYearStart = null,
        int? schoolYearEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active enrollments for a study load owned by a student.
    /// </summary>
    Task<IReadOnlyList<StudentCourse>> GetAllByStudyLoadIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an enrollment for a course and study load, including archived rows when requested.
    /// </summary>
    Task<StudentCourse?> GetByCourseAndStudyLoadAsync(long courseId, long studyLoadId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an active enrollment exists.
    /// </summary>
    Task<bool> ExistsByIdAsync(long studentCourseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new enrollment.
    /// </summary>
    Task<StudentCourse> AddAsync(StudentCourse studentCourse, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists updates to an existing enrollment.
    /// </summary>
    Task UpdateAsync(StudentCourse studentCourse, CancellationToken cancellationToken = default);
}
