namespace  EducAIte.Application.Extensions;


using EducAIte.Domain.Entities;



public static class AuthExtension
{
    public static bool IsValidPassword(this Student student, string inputPassword)
    {
        return BCrypt.Net.BCrypt.Verify(inputPassword, student.PasswordHash);
    }
}