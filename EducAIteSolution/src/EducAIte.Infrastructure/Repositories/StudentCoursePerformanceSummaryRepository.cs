using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class StudentCoursePerformanceSummaryRepository : IStudentCoursePerformanceSummaryRepository
{
    private readonly ApplicationDbContext _context;

    public StudentCoursePerformanceSummaryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentCoursePerformanceSummary?> GetTrackedByStudentCourseIdAsync(long studentCourseId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentCoursePerformanceSummaries
            .FirstOrDefaultAsync(summary => summary.StudentCourseId == studentCourseId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentCoursePerformanceSummary>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentCoursePerformanceSummaries
            .AsNoTracking()
            .Include(summary => summary.StudentCourse)
            .ThenInclude(studentCourse => studentCourse.Course)
            .Include(summary => summary.StudentCourse)
            .ThenInclude(studentCourse => studentCourse.StudyLoad)
            .Where(summary => summary.StudentCourse.StudyLoad.StudentId == studentId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StudentCoursePerformanceSummary summary, CancellationToken cancellationToken = default)
    {
        await _context.StudentCoursePerformanceSummaries.AddAsync(summary, cancellationToken);
    }

    public async Task UpdateAsync(StudentCoursePerformanceSummary summary, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
