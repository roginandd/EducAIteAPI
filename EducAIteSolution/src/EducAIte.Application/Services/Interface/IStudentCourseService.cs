using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;

/// <summary>
/// Defines application operations for student course enrollments.
/// </summary>
public interface IStudentCourseService
{
    /// <summary>
    /// Retrieves a specific enrollment owned by the authenticated student.
    /// </summary>
    Task<StudentCourseResponse?> GetByIdAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all enrollments owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<StudentCourseResponse>> GetMineAsync(long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all enrollments for a study load owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<StudentCourseResponse>> GetByStudyLoadAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or restores an enrollment for the authenticated student.
    /// </summary>
    Task<StudentCourseResponse> CreateAsync(CreateStudentCourseRequest request, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives an enrollment owned by the authenticated student.
    /// </summary>
    Task<bool> DeleteAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default);
}
