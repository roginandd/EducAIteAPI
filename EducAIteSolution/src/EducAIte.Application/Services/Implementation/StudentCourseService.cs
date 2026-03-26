using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.Grade;
using EducAIte.Domain.Exceptions.StudentCourse;
using EducAIte.Domain.Interfaces;

namespace EducAIte.Application.Services.Implementation;

/// <summary>
/// Orchestrates student course enrollment and grade use cases.
/// </summary>
public sealed class StudentCourseService : IStudentCourseService
{
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IStudyLoadRepository _studyLoadRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentCourseService> _logger;

    /// <summary>
    /// Initializes a new service instance.
    /// </summary>
    /// <param name="studentCourseRepository">The enrollment repository.</param>
    /// <param name="studyLoadRepository">The study load repository.</param>
    /// <param name="courseRepository">The course repository.</param>
    /// <param name="sqidService">The sqid encoder and decoder.</param>
    /// <param name="unitOfWork">The application unit of work.</param>
    /// <param name="logger">The service logger.</param>
    public StudentCourseService(
        IStudentCourseRepository studentCourseRepository,
        IStudyLoadRepository studyLoadRepository,
        ICourseRepository courseRepository,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<StudentCourseService> logger)
    {
        _studentCourseRepository = studentCourseRepository;
        _studyLoadRepository = studyLoadRepository;
        _courseRepository = courseRepository;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StudentCourseResponse?> GetByIdAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(studentCourseSqid, out long studentCourseId))
        {
            throw new InvalidSqidException(nameof(studentCourseSqid));
        }

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is not null)
        {
            return studentCourse.ToResponse(_sqidService);
        }

        bool exists = await _studentCourseRepository.ExistsByIdAsync(studentCourseId, cancellationToken);
        if (exists)
        {
            throw new StudentCourseForbiddenException();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourseResponse>> GetMineAsync(
        long studentId,
        GetStudentCoursesRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        request ??= new GetStudentCoursesRequest();

        Semester? semester = NormalizeSemester(request.Semester);
        ValidateSchoolYear(request.SchoolYearStart, request.SchoolYearEnd);

        IReadOnlyList<StudentCourse> studentCourses = await _studentCourseRepository.GetAllByStudentIdAsync(
            studentId,
            semester,
            request.SchoolYearStart,
            request.SchoolYearEnd,
            cancellationToken);
        return studentCourses
            .Select(studentCourse => studentCourse.ToResponse(_sqidService))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourseResponse>> GetByStudyLoadAsync(string studyLoadSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(studyLoadSqid, out long studyLoadId))
        {
            throw new InvalidSqidException(nameof(studyLoadSqid));
        }

        await GetOwnedStudyLoadOrThrowAsync(studyLoadId, studentId, cancellationToken);
        IReadOnlyList<StudentCourse> studentCourses = await _studentCourseRepository.GetAllByStudyLoadIdAndStudentIdAsync(studyLoadId, studentId, cancellationToken);

        return studentCourses
            .Select(studentCourse => studentCourse.ToResponse(_sqidService))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<StudentCourseResponse> CreateAsync(CreateStudentCourseRequest request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStudentIdIsValid(studentId);

        long courseId = DecodeSqidOrThrow(request.CourseSqid, nameof(request.CourseSqid));
        long studyLoadId = DecodeSqidOrThrow(request.StudyLoadSqid, nameof(request.StudyLoadSqid));

        await GetOwnedStudyLoadOrThrowAsync(studyLoadId, studentId, cancellationToken);
        await GetCourseOrThrowAsync(courseId, cancellationToken);

        StudentCourse? existing = await _studentCourseRepository.GetByCourseAndStudyLoadAsync(
            courseId,
            studyLoadId,
            includeDeleted: true,
            cancellationToken: cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (existing is not null)
            {
                if (!existing.IsDeleted)
                {
                    throw new CourseAlreadyEnrolledException();
                }

                existing.Restore();
                await _studentCourseRepository.UpdateAsync(existing, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Restored student course {StudentCourseId} for study load {StudyLoadId}",
                    existing.StudentCourseId,
                    existing.StudyLoadId);

                return existing.ToResponse(_sqidService);
            }

            // Keep the insert graph limited to StudentCourse so EF does not try to re-insert
            // the existing Course or StudyLoad rows that are only being referenced by FK.
            StudentCourse studentCourse = request.ToEntity(courseId, studyLoadId);

            StudentCourse created = await _studentCourseRepository.AddAsync(studentCourse, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            StudentCourse? reloaded = await _studentCourseRepository.GetByIdAndStudentIdAsync(created.StudentCourseId, studentId, cancellationToken);
            if (reloaded is null)
            {
                throw new InvalidOperationException("Student course was created but could not be reloaded.");
            }

            _logger.LogInformation(
                "Created student course {StudentCourseId} for study load {StudyLoadId}",
                created.StudentCourseId,
                created.StudyLoadId);

            return reloaded.ToResponse(_sqidService);
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while creating or restoring a student course for student {StudentId}", studentId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(studentCourseSqid, out long studentCourseId))
        {
            throw new InvalidSqidException(nameof(studentCourseSqid));
        }

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is null)
        {
            bool exists = await _studentCourseRepository.ExistsByIdAsync(studentCourseId, cancellationToken);
            if (exists)
            {
                throw new StudentCourseForbiddenException();
            }

            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            studentCourse.MarkDeleted();
            await _studentCourseRepository.UpdateAsync(studentCourse, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Archived student course {StudentCourseId}", studentCourseId);
            return true;
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while deleting student course {StudentCourseId}", studentCourseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GradeResponseDTO>> GetGradesAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(studentCourseSqid, studentId, cancellationToken);
        return studentCourse.GetActiveGrades().ToResponses(_sqidService);
    }

    /// <inheritdoc />
    public async Task<GradeResponseDTO?> GetGradeByTypeAsync(string studentCourseSqid, GradeType gradeType, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        EnsureValidGradeType(gradeType);

        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(studentCourseSqid, studentId, cancellationToken);
        Grade? grade = studentCourse.GetGrade(gradeType);
        return grade?.ToResponse(_sqidService);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GradeResponseDTO>> GetGradesByTypeAsync(GetGradesByTypeDTO request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStudentIdIsValid(studentId);

        IReadOnlyCollection<GradeType> gradeTypes = NormalizeGradeTypes(request.GradeTypes);
        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(request.StudentCourseSqid, studentId, cancellationToken);

        return studentCourse
            .GetGradesByTypes(gradeTypes)
            .ToResponses(_sqidService);
    }

    /// <inheritdoc />
    public async Task<GradeResponseDTO> CreateGradeAsync(CreateGradeDTO request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStudentIdIsValid(studentId);
        EnsureValidGradeType(request.GradeType);

        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(request.StudentCourseSqid, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            Grade grade = studentCourse.AddGrade(request.GradeType, request.GradeValue);
            await _studentCourseRepository.UpdateAsync(studentCourse, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Created or restored grade {GradeType} for student course {StudentCourseId}",
                request.GradeType,
                studentCourse.StudentCourseId);

            return grade.ToResponse(_sqidService);
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while creating grade {GradeType} for student course {StudentCourseSqid}", request.GradeType, request.StudentCourseSqid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GradeResponseDTO> UpdateGradeAsync(UpdateGradeDTO request, long studentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStudentIdIsValid(studentId);
        EnsureValidGradeType(request.GradeType);

        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(request.StudentCourseSqid, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            Grade grade = studentCourse.UpdateGrade(request.GradeType, request.GradeValue);
            await _studentCourseRepository.UpdateAsync(studentCourse, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Updated grade {GradeType} for student course {StudentCourseId}",
                request.GradeType,
                studentCourse.StudentCourseId);

            return grade.ToResponse(_sqidService);
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while updating grade {GradeType} for student course {StudentCourseSqid}", request.GradeType, request.StudentCourseSqid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteGradeAsync(string studentCourseSqid, GradeType gradeType, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        EnsureValidGradeType(gradeType);

        StudentCourse studentCourse = await GetOwnedStudentCourseBySqidOrThrowAsync(studentCourseSqid, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            bool deleted = studentCourse.RemoveGrade(gradeType);
            if (!deleted)
            {
                await SafeRollbackAsync(cancellationToken);
                return false;
            }

            await _studentCourseRepository.UpdateAsync(studentCourse, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Archived grade {GradeType} for student course {StudentCourseId}",
                gradeType,
                studentCourse.StudentCourseId);

            return true;
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error occurred while deleting grade {GradeType} for student course {StudentCourseSqid}", gradeType, studentCourseSqid);
            throw;
        }
    }

    private async Task<StudyLoad> GetOwnedStudyLoadOrThrowAsync(long studyLoadId, long studentId, CancellationToken cancellationToken)
    {
        StudyLoad? studyLoad = await _studyLoadRepository.GetByIdAndStudentIdAsync(studyLoadId, studentId, cancellationToken);
        if (studyLoad is not null)
        {
            return studyLoad;
        }

        if (studyLoad is null)
        {
            throw new StudyLoadNotFoundException(studyLoadId);
        }

        if (studyLoad.StudentId != studentId)
        {
            throw new StudyLoadForbiddenException();
        }

        return studyLoad;
    }

    private async Task<StudentCourse> GetOwnedStudentCourseBySqidOrThrowAsync(string studentCourseSqid, long studentId, CancellationToken cancellationToken)
    {
        if (!_sqidService.TryDecode(studentCourseSqid, out long studentCourseId))
        {
            throw new InvalidSqidException(nameof(studentCourseSqid));
        }

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(studentCourseId, studentId, cancellationToken);
        if (studentCourse is not null)
        {
            return studentCourse;
        }

        bool exists = await _studentCourseRepository.ExistsByIdAsync(studentCourseId, cancellationToken);
        if (exists)
        {
            throw new StudentCourseForbiddenException();
        }

        throw new StudentCourseNotFoundException(studentCourseSqid);
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

    private long DecodeSqidOrThrow(string sqid, string paramName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new InvalidSqidException(paramName);
        }

        return id;
    }

    private async Task SafeRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }
        catch (Exception rollbackException)
        {
            _logger.LogWarning(rollbackException, "Rollback failed for student course transaction.");
        }
    }

    private static IReadOnlyCollection<GradeType> NormalizeGradeTypes(IReadOnlyCollection<GradeType> gradeTypes)
    {
        ArgumentNullException.ThrowIfNull(gradeTypes);

        if (gradeTypes.Count == 0)
        {
            throw new ArgumentException("At least one grade type must be provided.", nameof(gradeTypes));
        }

        GradeType[] normalized = gradeTypes
            .Distinct()
            .ToArray();

        foreach (GradeType gradeType in normalized)
        {
            EnsureValidGradeType(gradeType);
        }

        return normalized;
    }

    private static void EnsureValidGradeType(GradeType gradeType)
    {
        bool isGradeTypeValid = Enum.IsDefined(typeof(GradeType), gradeType);

        if (!isGradeTypeValid)
        {
            throw new InvalidGradeTypeException();
        }
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }

    private static Semester? NormalizeSemester(int? semester)
    {
        if (!semester.HasValue)
        {
            return null;
        }

        if (!Enum.IsDefined(typeof(Semester), semester.Value))
        {
            throw new ArgumentException("Semester is invalid.", nameof(semester));
        }

        return (Semester)semester.Value;
    }

    private static void ValidateSchoolYear(int? schoolYearStart, int? schoolYearEnd)
    {
        bool hasSchoolYearStart = schoolYearStart.HasValue;
        bool hasSchoolYearEnd = schoolYearEnd.HasValue;

        if (hasSchoolYearStart != hasSchoolYearEnd)
        {
            throw new ArgumentException("SchoolYearStart and SchoolYearEnd must both be provided when filtering by school year.");
        }

        if (hasSchoolYearStart && schoolYearEnd != schoolYearStart + 1)
        {
            throw new ArgumentException("School year is invalid.");
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
