using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Exceptions.Base;
using EducAIte.Domain.Exceptions.Student;
using EducAIte.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EducAIte.Application.Services.Implementation;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository studentRepository,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
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

        var existingStudentWithEmail = await _studentRepository.GetByEmailAsync(request.Email);
        if (existingStudentWithEmail is not null)
        {
            throw new StudentEmailAlreadyExistsException("Email is already registered.");
        }

        var student = request.toEntity();
        student.IsDeleted = false;

        Student createdStudent;
        try
        {
            createdStudent = await _studentRepository.AddStudentAsync(student);
        }
        catch (DbUpdateException ex) when (IsStudentEmailConstraintViolation(ex))
        {
            throw new StudentEmailAlreadyExistsException("Email is already registered.");
        }

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

    public async Task ArchiveCurrentStudentAsync(long studentId, CancellationToken cancellationToken = default)
    {
        Student? student = await _studentRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            throw new StudentNotFoundException($"Student with ID {studentId} not found.");
        }

        await ArchiveStudentAsync(student, cancellationToken);
        _logger.LogInformation("Student {StudentId} was archived.", studentId);
    }

    public async Task RollbackRegisteredStudentAsync(
        string studentSqid,
        RollbackStudentRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(studentSqid, out long studentId))
        {
            throw new InvalidSqidException($"Student sqid '{studentSqid}' is invalid.");
        }

        Student? student = await _studentRepository.GetTrackedByStudentIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            throw new StudentNotFoundException($"Student with ID {studentId} not found.");
        }

        if (!string.Equals(student.StudentIdNumber, request.StudentIdNumber, StringComparison.OrdinalIgnoreCase))
        {
            throw new StudentValidationException("Student rollback credentials are invalid.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
        {
            throw new StudentValidationException("Student rollback credentials are invalid.");
        }

        await ArchiveStudentAsync(student, cancellationToken);
        _logger.LogInformation("Student {StudentId} was archived through registration rollback.", studentId);
    }

    private async Task ArchiveStudentAsync(Student student, CancellationToken cancellationToken)
    {
        student.IsDeleted = true;
        student.UpdatedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsStudentEmailConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
               string.Equals(postgresException.ConstraintName, "UX_Students_Email", StringComparison.Ordinal);
    }
}
