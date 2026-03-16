namespace EducAIte.Infrastructure.Repositories;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class StudyLoadRepository(ApplicationDbContext dbContext) : IStudyLoadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<StudyLoad?> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudyLoads
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
    }

    public async Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _dbContext.StudyLoads.Add(studyLoad);
        return studyLoad;
    }

    public async Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _dbContext.StudyLoads.Update(studyLoad);
        return studyLoad;
    }

    public async Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default)
    {
        var studyLoad = await _dbContext.StudyLoads.FindAsync(new object[] { id }, cancellationToken);
        if (studyLoad == null) return false;

        _dbContext.StudyLoads.Remove(studyLoad);
        return true;
    }
}