namespace EducAIte.Domain.Interfaces;

using EducAIte.Domain.Entities;

public interface IStudyLoadRepository
{
    public Task<StudyLoad?> GetStudyLoadByIdAsync(long studyLoadId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<StudyLoad>> GetAllStudyLoadsByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);
    public Task<StudyLoad?> GetByIdAndStudentIdAsync(long studentId, long studyloadId, CancellationToken cancellationToken = default);
    public Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);
    public Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);
    public Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default);
}
