namespace EducAIte.Application.Services.Interface;

using EducAIte.Application.DTOs;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using System.Threading;
using EducAIte.Domain.Entities;
public interface IStudyLoadService
{
    Task<StudyLoadResponse?> GetStudyLoadByIdAsync(string studyLoadSqid, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudyLoadResponse>> GetAllStudyLoadsByStudentIdAsync(string studentSqid, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse?> GetByIdAndStudentIdAsync(string studentSqid, string studyLoadSqid, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse?> GetByIdAndStudentIdAsync(long studentId, string studyLoadSqid, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse> ApplyParsedCoursesAsync(string studyLoadSqid, ApplyParsedStudyLoadCoursesRequest request, long studentId, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse> UpdateStudyLoadAsync(StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStudyLoadAsync(string studyLoadSqid, CancellationToken cancellationToken = default);
}
