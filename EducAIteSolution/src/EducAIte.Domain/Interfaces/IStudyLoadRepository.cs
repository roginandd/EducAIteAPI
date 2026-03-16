namespace EducAIte.Domain.Interfaces;

using EducAIte.Domain.Entities;

public interface IStudyLoadRepository
{
    public Task<List<StudyLoad>> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);
    public Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);
    public Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);
    public Task DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default);
}