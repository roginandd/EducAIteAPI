namespace EducAIte.Infrastructure.Repositories;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class StudyLoadRepository(ApplicationDbContext context) : IStudyLoadRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<StudyLoad>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudyLoads.AsNoTracking().Where(s => s.StudentId == studentId).ToListAsync(cancellationToken);
    }

    public async Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _context.StudyLoads.Add(studyLoad);
        return studyLoad;
    }

    public async Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default)
    {
        _context.StudyLoads.Update(studyLoad);
        return studyLoad;
    }

    public async Task DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.StudyLoads
            .Where(sl => sl.StudyLoadId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(sl => sl.IsDeleted, true),
                cancellationToken);
        
        if (affectedRows == 0)            
            throw new KeyNotFoundException($"StudyLoad with id {id} not found.");
    }
}