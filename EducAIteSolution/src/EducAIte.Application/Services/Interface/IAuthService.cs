namespace EducAIte.Application.Interfaces;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;


public interface IAuthService
{
    Task<AuthResult> Login (LoginRequest loginRequest);
}