using EducAIte.Application.DTOs.Response;

namespace EducAIte.Application.Services.Interface;


public interface IStudentService
{
    Task<StudentResponse> GetStudentByIdAsync(long studentId);
    
}