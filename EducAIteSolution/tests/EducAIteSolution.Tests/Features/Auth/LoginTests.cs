using Castle.Core.Logging;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EducAIteSolution.Tests.Features.Auth;

public class LoginTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly AuthService _authService;
    private readonly Mock<ILogger<AuthService>> _mockLogger; // <--- FIX: Keep the Mock wrapper
    
    private readonly ITestOutputHelper _output;
    public LoginTests(ITestOutputHelper output)
    {
        _output = output;
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("supersecretkeythatislongenoughforhmacsha256");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpiryInMinutes"]).Returns("60");

        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(_mockConfiguration.Object, _mockStudentRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var loginRequest = new LoginRequest("12345", "password123");
        var student = new Student
        {
            StudentId = 1,
            StudentIdNumber = "12345",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };

        _mockStudentRepository.Setup(r => r.GetByStudentIdNumberAsync("12345")).ReturnsAsync(student);

        // Act
        var result = await _authService.Login(loginRequest);

        _output.WriteLine("Login Result: Success={0}, Token={1}, Expiration={2}, Error={3}", 
            result.Success, result.Token, result.Expiration, result.Error);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.Expiration);
        Assert.Null(result.Error);

        // Validate the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(result.Token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == "student_id" && c.Value == "12345");
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "1");
    }

    [Fact]
    public async Task Login_WithInvalidStudentIdNumber_ReturnsFailureResult()
    {
        // Arrange
        var loginRequest = new LoginRequest("invalid", "password");

        _mockStudentRepository.Setup(r => r.GetByStudentIdNumberAsync("invalid")).ReturnsAsync((Student?)null);

        // Act
        var result = await _authService.Login(loginRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Null(result.Expiration);
        Assert.Equal("Authentication failed. Student not found.", result.Error);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var loginRequest = new LoginRequest("12345", "wrongpassword");
        var student = new Student
        {
            StudentId = 1,
            StudentIdNumber = "12345",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };

        _mockStudentRepository.Setup(r => r.GetByStudentIdNumberAsync("12345")).ReturnsAsync(student);

        // Act
        var result = await _authService.Login(loginRequest);


        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Null(result.Expiration);
        Assert.Equal("Invalid Student ID Number or Password.", result.Error);
    }
}