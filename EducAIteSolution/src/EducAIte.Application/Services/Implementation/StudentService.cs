using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.Student;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISqidService _sqidService;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository studentRepository, ISqidService sqidService, ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _sqidService = sqidService;
        _logger = logger;
    }

    public async Task<StudentResponse> RegisterStudentAsync(StudentRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new StudentValidationException("Password is required.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new StudentValidationException("Password and ConfirmPassword do not match.");
        }

        var existingStudent = await _studentRepository.GetByStudentIdNumberAsync(request.StudentIdNumber);
        if (existingStudent is not null)
        {
            throw new StudentAlreadyExistsException("Student ID Number is already registered.");
        }

        var student = request.toEntity();
        student.IsDeleted = false;

        var createdStudent = await _studentRepository.AddStudentAsync(student);
        _logger.LogInformation("Student registered successfully with StudentIdNumber {StudentIdNumber}.", createdStudent.StudentIdNumber);

        return createdStudent.ToDTO(_sqidService);
    }

    public async Task<StudentResponse> GetStudentBySqidAsync(string studentSqid)
    {
        if (!_sqidService.TryDecode(studentSqid, out long studentId))
        {
            throw new InvalidSqidException($"Student sqid '{studentSqid}' is invalid.");
        }

        return await GetCurrentStudentAsync(studentId);
    }

    public async Task<StudentResponse> GetCurrentStudentAsync(long studentId)
    {
        var student = await _studentRepository.GetByStudentIdAsync(studentId);
        if (student is null)
        {
            throw new StudentNotFoundException($"Student with ID {studentId} not found.");
        }

        return student.ToDTO(_sqidService);
    }
}
