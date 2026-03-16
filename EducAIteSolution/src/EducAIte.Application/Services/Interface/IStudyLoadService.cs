namespace EducAIte.Application.Services.Interface;

using EducAIte.Application.DTOs;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using System.Threading;
using EducAIte.Domain.Entities;
public interface IStudyLoadService
{
    Task<List<StudyLoadResponse>> GetStudyLoadByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default);
    Task<StudyLoadResponse> UpdateStudyLoadAsync(long id, StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default);
}