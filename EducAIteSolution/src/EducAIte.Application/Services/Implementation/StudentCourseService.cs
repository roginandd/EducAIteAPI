using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

/// <summary>
/// Orchestrates student course enrollment use cases.
/// </summary>
public sealed class StudentCourseService : IStudentCourseService
{
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IStudyLoadRepository _studyLoadRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<StudentCourseService> _logger;

    /// <summary>
    /// Initializes a new service instance.
    /// </summary>
    /// <param name="studentCourseRepository">The enrollment repository.</param>
    /// <param name="studyLoadRepository">The study load repository.</param>
    /// <param name="courseRepository">The course repository.</param>
    /// <param name="logger">The service logger.</param>
    public StudentCourseService(
        IStudentCourseRepository studentCourseRepository,
        IStudyLoadRepository studyLoadRepository,
        ICourseRepository courseRepository,
        ILogger<StudentCourseService> logger)
    {
        _studentCourseRepository = studentCourseRepository;
        _studyLoadRepository = studyLoadRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StudentCourseResponse?> GetByIdAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        EnsurePositiveId(studentCourseId, nameof(studentCourseId));

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is not null)
        {
            return studentCourse.ToResponse();
        }

        bool exists = await _studentCourseRepository.ExistsByIdAsync(studentCourseId, cancellationToken);
        if (exists)
        {
            throw new UnauthorizedAccessException("Student course does not belong to the authenticated student.");
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourseResponse>> GetMineAsync(long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        IReadOnlyList<StudentCourse> studentCourses = await _studentCourseRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        return studentCourses
            .Select(studentCourse => studentCourse.ToResponse())
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourseResponse>> GetByStudyLoadAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        EnsurePositiveId(studyLoadId, nameof(studyLoadId));

        await GetOwnedStudyLoadOrThrowAsync(studyLoadId, studentId, cancellationToken);
        IReadOnlyList<StudentCourse> studentCourses = await _studentCourseRepository.GetAllByStudyLoadIdAndStudentIdAsync(studyLoadId, studentId, cancellationToken);

        return studentCourses
            .Select(studentCourse => studentCourse.ToResponse())
            .ToList();
    }

    /// <inheritdoc />
    public async Task<StudentCourseResponse> CreateAsync(CreateStudentCourseRequest request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStudentIdIsValid(studentId);

        StudyLoad studyLoad = await GetOwnedStudyLoadOrThrowAsync(request.StudyLoadId, studentId, cancellationToken);
        Course course = await GetCourseOrThrowAsync(request.CourseId, cancellationToken);

        StudentCourse? existing = await _studentCourseRepository.GetByCourseAndStudyLoadAsync(
            request.CourseId,
            request.StudyLoadId,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsDeleted)
            {
                throw new InvalidOperationException("The course is already enrolled for the selected study load.");
            }

            existing.Restore();
            await _studentCourseRepository.UpdateAsync(existing, cancellationToken);
            _logger.LogInformation(
                "Restored student course {StudentCourseId} for study load {StudyLoadId}",
                existing.StudentCourseId,
                existing.StudyLoadId);

            return existing.ToResponse();
        }

        StudentCourse studentCourse = request.ToEntity();
        studentCourse.AssignCourse(course);
        studentCourse.AssignStudyLoad(studyLoad);

        StudentCourse created = await _studentCourseRepository.AddAsync(studentCourse, cancellationToken);
        _logger.LogInformation(
            "Created student course {StudentCourseId} for study load {StudyLoadId}",
            created.StudentCourseId,
            created.StudyLoadId);

        return created.ToResponse();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        EnsurePositiveId(studentCourseId, nameof(studentCourseId));

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            bool exists = await _studentCourseRepository.ExistsByIdAsync(studentCourseId, cancellationToken);
            if (exists)
            {
                throw new UnauthorizedAccessException("Student course does not belong to the authenticated student.");
            }

            return false;
        }

        studentCourse.MarkDeleted();
        await _studentCourseRepository.UpdateAsync(studentCourse, cancellationToken);
        _logger.LogInformation("Archived student course {StudentCourseId}", studentCourseId);
        return true;
    }

    private async Task<StudyLoad> GetOwnedStudyLoadOrThrowAsync(long studyLoadId, long studentId, CancellationToken cancellationToken)
    {
        // StudyLoad? studyLoad = await _studyLoadRepository.GetByIdAndStudentIdAsync(studyLoadId, studentId, cancellationToken);
        // if (studyLoad is not null)
        // {
        //     return studyLoad;
        // }

        StudyLoad? existingStudyLoad = await _studyLoadRepository.GetStudyLoadByIdAsync(studyLoadId, cancellationToken);
        if (existingStudyLoad is not null)
        {
            throw new UnauthorizedAccessException("Study load does not belong to the authenticated student.");
        }

        throw new KeyNotFoundException($"Study load with ID {studyLoadId} was not found.");
    }

    private async Task<Course> GetCourseOrThrowAsync(long courseId, CancellationToken cancellationToken)
    {
        EnsurePositiveId(courseId, nameof(courseId));

        Course? course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is not null)
        {
            return course;
        }

        throw new KeyNotFoundException($"Course with ID {courseId} was not found.");
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }

    private static void EnsurePositiveId(long value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }
    }
}
