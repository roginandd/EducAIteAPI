namespace EducAIte.Application.Services.Interface;

using EducAIte.Application.DTOs;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using System.Threading;
using EducAIte.Domain.Entities;
public interface IStudyLoadService
{
    Task<StudyLoadDto?> GetStudyLoadByIdAsync(string studyLoadSqid, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudyLoadDto>> GetAllStudyLoadsByStudentIdAsync(string studentSqid, CancellationToken cancellationToken = default);
    Task<StudyLoadDto> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default);
    Task<StudyLoadDto> UpdateStudyLoadAsync(StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStudyLoadAsync(string studyLoadSqid, CancellationToken cancellationToken = default);
}