using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

/// <summary>
/// Implements student course persistence using Entity Framework Core.
/// </summary>
public sealed class StudentCourseRepository : IStudentCourseRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new repository instance.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public StudentCourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<StudentCourse?> GetByIdAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(studentCourse => studentCourse.StudentCourseId == studentCourseId && !studentCourse.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StudentCourse?> GetByIdAndStudentIdAsync(long studentCourseId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .FirstOrDefaultAsync(
                studentCourse => studentCourse.StudentCourseId == studentCourseId
                    && !studentCourse.IsDeleted
                    && !studentCourse.StudyLoad.IsDeleted
                    && studentCourse.StudyLoad.StudentId == studentId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourse>> GetAllByStudentIdAsync(
        long studentId,
        Semester? semester = null,
        int? schoolYearStart = null,
        int? schoolYearEnd = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StudentCourse> query = CreateBaseQuery()
            .AsNoTracking()
            .Where(studentCourse => !studentCourse.IsDeleted)
            .Where(studentCourse => !studentCourse.StudyLoad.IsDeleted)
            .Where(studentCourse => studentCourse.StudyLoad.StudentId == studentId);

        if (semester.HasValue)
        {
            query = query.Where(studentCourse => studentCourse.StudyLoad.Semester == semester.Value);
        }

        if (schoolYearStart.HasValue && schoolYearEnd.HasValue)
        {
            query = query.Where(studentCourse =>
                studentCourse.StudyLoad.SchoolYearStart == schoolYearStart.Value &&
                studentCourse.StudyLoad.SchoolYearEnd == schoolYearEnd.Value);
        }

        return await query
            .OrderByDescending(studentCourse => studentCourse.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentCourse>> GetAllByStudyLoadIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default)
    {
        return await CreateBaseQuery()
            .AsNoTracking()
            .Where(studentCourse => !studentCourse.IsDeleted)
            .Where(studentCourse => !studentCourse.StudyLoad.IsDeleted)
            .Where(studentCourse => studentCourse.StudyLoadId == studyLoadId)
            .Where(studentCourse => studentCourse.StudyLoad.StudentId == studentId)
            .OrderByDescending(studentCourse => studentCourse.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StudentCourse?> GetByCourseAndStudyLoadAsync(
        long courseId,
        long studyLoadId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StudentCourse> query = CreateBaseQuery()
            .Where(studentCourse => studentCourse.CourseId == courseId && studentCourse.StudyLoadId == studyLoadId);

        if (!includeDeleted)
        {
            query = query.Where(studentCourse => !studentCourse.IsDeleted);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentCourses
            .AsNoTracking()
            .AnyAsync(studentCourse => studentCourse.StudentCourseId == studentCourseId && !studentCourse.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public Task<StudentCourse> AddAsync(StudentCourse studentCourse, CancellationToken cancellationToken = default)
    {
        _context.StudentCourses.Add(studentCourse);
        return Task.FromResult(studentCourse);
    }

    /// <inheritdoc />
    public Task UpdateAsync(StudentCourse studentCourse, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private IQueryable<StudentCourse> CreateBaseQuery()
    {
        return _context.StudentCourses
            .Include(studentCourse => studentCourse.Course)
            .Include(studentCourse => studentCourse.Grades)
            .Include(studentCourse => studentCourse.StudyLoad);
    }
}
