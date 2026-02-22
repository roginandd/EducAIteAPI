using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Interfaces;
using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EducAIteSolution.Tests.Integration.Features.Auth;

public class AuthIntegrationTests : IntegrationTestBase
{
    private readonly IAuthService _authService;
    private readonly ITestOutputHelper _outputHelper;

    public AuthIntegrationTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _authService = ServiceProvider.GetRequiredService<IAuthService>();
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var student = new Student
        {
            StudentIdNumber = "12345",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        ApplicationDbContext.Students.Add(student);
        await ApplicationDbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest("12345", "password123");

        // Act
        var result = await _authService.Login(loginRequest);


        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.Expiration);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var studentRegistrationRequest = new StudentRegistrationRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            StudentIdNumber = "67890"
        };

        // Act
        var result = await _authService.Register(studentRegistrationRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.Expiration);
        Assert.Null(result.Error);

        // Verify the student was added to the database
        var addedStudent = await ApplicationDbContext.Students.FirstOrDefaultAsync(s => s.StudentIdNumber == "67890");

        
        _outputHelper.WriteLine($"Added student: {addedStudent.ToString()}");

        Assert.NotNull(addedStudent);
        Assert.Equal("John", addedStudent.FirstName);
        Assert.Equal("Doe", addedStudent.LastName);
        Assert.Equal("john.doe@example.com", addedStudent.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", addedStudent.PasswordHash));
    }

    [Fact]
    public async Task GetProfile_WithValidStudentIdNumber_ReturnsProfile()
    {
        // Arrange: Register a student first
        var studentRegistrationRequest = new StudentRegistrationRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "password456",
            ConfirmPassword = "password456",
            StudentIdNumber = "54321"
        };

        await _authService.Register(studentRegistrationRequest);

        // Act
        var profile = await _authService.GetProfile("54321");

        // Assert
        Assert.NotNull(profile);
        Assert.Equal("54321", profile.StudentIdNumber);
        Assert.Equal("Jane", profile.FirstName);
        Assert.Equal("Smith", profile.LastName);
        Assert.Equal("jane.smith@example.com", profile.Email);

        // Log the profile
        _outputHelper.WriteLine($"Retrieved profile: StudentId={profile.StudentId}, Name={profile.FirstName} {profile.LastName}, Email={profile.Email}");
    }
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var student = new Student
        {
            StudentIdNumber = "12345",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        ApplicationDbContext.Students.Add(student);
        await ApplicationDbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest("12345", "wrongpassword");

        // Act
        var result = await _authService.Login(loginRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Null(result.Expiration);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid", result.Error);
    }

    [Fact]
    public async Task Login_WithNonExistentStudent_ReturnsFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest("99999", "password123");

        // Act
        var result = await _authService.Login(loginRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Null(result.Expiration);
        Assert.NotNull(result.Error);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Register_WithDuplicateStudentIdNumber_ReturnsFailure()
    {
        // Arrange: Register first student
        var request1 = new StudentRegistrationRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "pass123",
            ConfirmPassword = "pass123",
            StudentIdNumber = "11111"
        };
        await _authService.Register(request1);

        // Try to register with same ID
        var request2 = new StudentRegistrationRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "pass123",
            ConfirmPassword = "pass123",
            StudentIdNumber = "11111"  // Duplicate
        };

        // Act
        var result = await _authService.Register(request2);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Null(result.Expiration);
        Assert.NotNull(result.Error);
        Assert.Contains("already registered", result.Error);
    }

    [Fact]
    public async Task GetProfile_WithInvalidStudentIdNumber_ReturnsNull()
    {
        // Act
        var profile = await _authService.GetProfile("");

        // Assert
        Assert.Null(profile);
    }

    [Fact]
    public async Task GetProfile_WithNonExistentStudentIdNumber_ReturnsNull()
    {
        // Act
        var profile = await _authService.GetProfile("nonexistent");

        // Assert
        Assert.Null(profile);
    }
}