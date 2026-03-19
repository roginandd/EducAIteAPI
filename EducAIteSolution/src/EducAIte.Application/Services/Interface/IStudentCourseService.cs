using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Enum;

namespace EducAIte.Application.Services.Interface;

/// <summary>
/// Defines application operations for student course enrollments and grades.
/// </summary>
public interface IStudentCourseService
{
    /// <summary>
    /// Retrieves a specific enrollment owned by the authenticated student.
    /// </summary>
    Task<StudentCourseResponse?> GetByIdAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all enrollments owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<StudentCourseResponse>> GetMineAsync(long studentId, GetStudentCoursesRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all enrollments for a study load owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<StudentCourseResponse>> GetByStudyLoadAsync(string studyLoadSqid, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or restores an enrollment for the authenticated student.
    /// </summary>
    Task<StudentCourseResponse> CreateAsync(CreateStudentCourseRequest request, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives an enrollment owned by the authenticated student.
    /// </summary>
    Task<bool> DeleteAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active grades for a student course owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<GradeResponseDTO>> GetGradesAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific active grade by grading period for a student course owned by the authenticated student.
    /// </summary>
    Task<GradeResponseDTO?> GetGradeByTypeAsync(string studentCourseSqid, GradeType gradeType, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active grades matching the requested grading periods for a student course owned by the authenticated student.
    /// </summary>
    Task<IReadOnlyList<GradeResponseDTO>> GetGradesByTypeAsync(GetGradesByTypeDTO request, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or restores a grade for a student course owned by the authenticated student.
    /// </summary>
    Task<GradeResponseDTO> CreateGradeAsync(CreateGradeDTO request, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing grade for a student course owned by the authenticated student.
    /// </summary>
    Task<GradeResponseDTO> UpdateGradeAsync(UpdateGradeDTO request, long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a grade for a student course owned by the authenticated student.
    /// </summary>
    Task<bool> DeleteGradeAsync(string studentCourseSqid, GradeType gradeType, long studentId, CancellationToken cancellationToken = default);
}
