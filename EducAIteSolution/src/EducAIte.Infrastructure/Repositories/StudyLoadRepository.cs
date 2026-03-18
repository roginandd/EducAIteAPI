namespace EducAIte.Infrastructure.Repositories;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class StudyLoadRepository(ApplicationDbContext dbContext) : IStudyLoadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<StudyLoad?> GetByIdAsync(long studyLoadId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .Include(studyLoad => studyLoad.Courses)
            .Include(studyLoad => studyLoad.FileMetadata)
            .FirstOrDefaultAsync(studyLoad => studyLoad.StudyLoadId == studyLoadId, cancellationToken);
    }

    public async Task<StudyLoad?> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .Include(studyLoad => studyLoad.Courses)
            .Include(studyLoad => studyLoad.FileMetadata)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
    }

    public async Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _dbContext.StudyLoads.Add(studyLoad);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return studyLoad;
    }

    public async Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _dbContext.StudyLoads.Update(studyLoad);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return studyLoad;
    }

    public async Task<StudyLoad?> GetByIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .Include(studyLoad => studyLoad.Courses)
            .Include(studyLoad => studyLoad.FileMetadata)
            .FirstOrDefaultAsync(studyLoad => studyLoad.StudyLoadId == studyLoadId && studyLoad.StudentId == studentId, cancellationToken);
    }

    public async Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default)
    {
        var studyLoad = await _dbContext.StudyLoads.FindAsync(new object[] { id }, cancellationToken);
        if (studyLoad == null) return false;

        _dbContext.StudyLoads.Remove(studyLoad);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
