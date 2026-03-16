namespace EducAIte.Application.DTOs.Response;

// Used for lists/cards (Fast loading)
public record StudentBriefResponse
{
    public string Sqid { get; init; } = string.Empty;
    public string StudentIdNumber { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Program { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

// Used for full profile view
public record StudentResponse
{
    public string Sqid { get; init; } = string.Empty;
    public long Id { get; init; }
    public string StudentIdNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Program { get; init; } = string.Empty;
    public int Semester { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record StudentProfileResponse
{
    public required string StudentSqid { get; init; }
    public required string StudentIdNumber { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    // Add other fields as needed, e.g., courses, etc.
}
