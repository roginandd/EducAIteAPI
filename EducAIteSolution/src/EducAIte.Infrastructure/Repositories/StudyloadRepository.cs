namespace EducAIte.Infrastructure.Repositories;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class StudyLoadRepository(ApplicationDbContext dbContext) : IStudyLoadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<StudyLoad?> GetStudyLoadByIdAsync(long studyLoadId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .Include(studyLoad => studyLoad.Courses)
            .Include(studyLoad => studyLoad.FileMetadata)
            .FirstOrDefaultAsync(studyLoad => studyLoad.StudyLoadId == studyLoadId && !studyLoad.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<StudyLoad>> GetAllStudyLoadsByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .Include(studyLoad => studyLoad.Courses)
            .Include(studyLoad => studyLoad.FileMetadata)
            .Where(s => s.StudentId == studentId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.StudyLoads.AddAsync(studyLoad, cancellationToken);
        
        return entry.Entity;
    }

    public async Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _dbContext.StudyLoads.Update(studyLoad);
        
        return studyLoad;
    }

    public async Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default)
    {
        var studyLoad = await _dbContext.StudyLoads.FindAsync([id], cancellationToken);

        if (studyLoad == null) return false;

        studyLoad.IsDeleted = true;
        _dbContext.StudyLoads.Update(studyLoad);

        return true;
    }
}
