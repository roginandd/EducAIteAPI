using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<StudentResponse> RegisterStudentAsync(StudentRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request));
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password and ConfirmPassword do not match.", nameof(request));
        }

        var existingStudent = await _studentRepository.GetByStudentIdNumberAsync(request.StudentIdNumber);
        if (existingStudent is not null)
        {
            throw new InvalidOperationException("Student ID Number is already registered.");
        }

        var student = request.toEntity();
        student.IsDeleted = false;

        var createdStudent = await _studentRepository.AddStudentAsync(student);
        _logger.LogInformation("Student registered successfully with StudentIdNumber {StudentIdNumber}.", createdStudent.StudentIdNumber);

        return createdStudent.ToDTO();
    }

    public async Task<StudentResponse> GetStudentByIdAsync(long studentId)
    {
        var student = await _studentRepository.GetByStudentIdAsync(studentId);
        if (student is null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        return student.ToDTO();
    }
}
