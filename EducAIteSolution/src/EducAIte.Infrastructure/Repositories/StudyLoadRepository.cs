using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

/// <summary>
/// Provides read access to study loads needed by application services.
/// </summary>
public sealed class StudyLoadRepository : IStudyLoadRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new repository instance.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public StudyLoadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<StudyLoad?> GetByIdAsync(long studyLoadId, CancellationToken cancellationToken = default)
    {
        return await _context.StudyLoads
            .AsNoTracking()
            .FirstOrDefaultAsync(studyLoad => studyLoad.StudyLoadId == studyLoadId && !studyLoad.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StudyLoad?> GetByIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudyLoads
            .AsNoTracking()
            .FirstOrDefaultAsync(
                studyLoad => studyLoad.StudyLoadId == studyLoadId
                    && !studyLoad.IsDeleted
                    && studyLoad.StudentId == studentId,
                cancellationToken);
    }
}
