using EducAIte.Application.DTOs.Response;
using EducAIte.Application.DTOs.Request;

namespace EducAIte.Application.Services.Interface;


public interface IStudentService
{
    Task<StudentResponse> RegisterStudentAsync(StudentRegistrationRequest request);
    Task<StudentResponse> GetStudentBySqidAsync(string studentSqid);
    Task<StudentResponse> GetCurrentStudentAsync(long studentId);
}
