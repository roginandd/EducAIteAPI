
namespace EducAIte.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;


/*
    
 */
public class AuthService : IAuthService
{
    // injection of configuration to access JWT credentials
    private readonly IConfiguration _configuration;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, IStudentRepository studentRepository, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    private string GenerateJwtToken(Student student)
    {
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, student.StudentId.ToString()),
            new Claim("student_id", student.StudentIdNumber)
        };
        
        var token = new JwtSecurityToken (
            issuer: _configuration["Jwt:Issuer"]!,
            audience: _configuration["Jwt:Audience"]!,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResult> Login (LoginRequest loginRequest)
    {   
        // Destructure the login request to get the student ID number and password
        string studentIdNumber = loginRequest.StudentIdNumber;
        string password = loginRequest.Password;

        if (string.IsNullOrEmpty(studentIdNumber))
            return AuthResult.Fail("Student ID Number is Required.");
        if (string.IsNullOrEmpty(password))
            return AuthResult.Fail("Password is Required.");

        Student? student = await _studentRepository.GetByStudentIdNumberAsync(studentIdNumber);

        if (student == null)
            return AuthResult.Fail("Authentication failed. Student not found.");

        bool isAuthenticated = student.IsValidPassword(password);

        if (!isAuthenticated)
            return AuthResult.Fail("Invalid Student ID Number or Password.");

        string token = GenerateJwtToken(student);

        _logger.LogInformation("Login successful for student ID {StudentId}", studentIdNumber);

        DateTime expiration = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]!));

        return AuthResult.Ok(token, expiration);
    }

    public async Task<AuthResult> Register (StudentRegistrationRequest studentRegistrationRequest)
    {       
        // Destructure the registration request to get the student ID number and password
        string studentIdNumber = studentRegistrationRequest.StudentIdNumber;

        // Check if the student ID number is already registered
        Student? existingStudentWithIdNumber = await _studentRepository.GetByStudentIdNumberAsync(studentIdNumber);

        if (existingStudentWithIdNumber != null)
            return AuthResult.Fail("Student ID Number is already registered.");

        Student newStudent = studentRegistrationRequest.toEntity();

        await _studentRepository.AddStudentAsync(newStudent);

        string token = GenerateJwtToken(newStudent);

        _logger.LogInformation("Registration successful for student ID {StudentId}", studentIdNumber);

        DateTime expiration = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]!));

        return AuthResult.Ok(token, expiration);
    }

    public async Task<StudentProfileResponse?> GetProfile(string studentIdNumber)
    {
        if (string.IsNullOrEmpty(studentIdNumber))
            return null;

        Student? student = await _studentRepository.GetByStudentIdNumberAsync(studentIdNumber);

        if (student == null)
            return null;

        _logger.LogInformation("Profile retrieved for student ID {StudentId}", studentIdNumber);

        return student.ToStudentProfile();
    }
        
}