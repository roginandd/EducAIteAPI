using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public sealed class StudentOverallPerformanceSummaryRepository : IStudentOverallPerformanceSummaryRepository
{
    private readonly ApplicationDbContext _context;

    public StudentOverallPerformanceSummaryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentOverallPerformanceSummary?> GetTrackedByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentOverallPerformanceSummaries
            .FirstOrDefaultAsync(summary => summary.StudentId == studentId, cancellationToken);
    }

    public async Task AddAsync(StudentOverallPerformanceSummary summary, CancellationToken cancellationToken = default)
    {
        await _context.StudentOverallPerformanceSummaries.AddAsync(summary, cancellationToken);
    }

    public async Task UpdateAsync(StudentOverallPerformanceSummary summary, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
