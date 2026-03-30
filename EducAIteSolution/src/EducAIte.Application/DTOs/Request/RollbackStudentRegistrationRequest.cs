namespace EducAIte.Application.DTOs.Request;

public sealed record RollbackStudentRegistrationRequest
{
    public string StudentIdNumber { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
