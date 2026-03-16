namespace EducAIte.Domain.Interfaces;

using EducAIte.Domain.Entities;

public interface IStudyLoadRepository
{
    public Task<StudyLoad?> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);
    public Task<StudyLoad> AddStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);
    public Task<StudyLoad> UpdateStudyLoadAsync(StudyLoad studyLoad, CancellationToken cancellationToken = default);

        Task<StudyLoad?> GetByIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default);

    public Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default);
}
