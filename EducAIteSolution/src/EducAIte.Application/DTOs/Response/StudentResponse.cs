namespace EducAIte.Application.DTOs.Response;

// Used for lists/cards (Fast loading)
public record StudentBriefResponse(
    long Id,
    string StudentIdNumber,
    string FullName,
    string Program,
    string Email
);

// Used for full profile view
public record StudentResponse(
    long Id,
    string StudentIdNumber,
    string FirstName,
    string LastName,
    string Program,
    int Semester,
    string Email,
    string PhoneNumber,
    DateTime CreatedAt
);

public record StudentProfileResponse
{
    public required long StudentId { get; init; }
    public required string StudentIdNumber { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    // Add other fields as needed, e.g., courses, etc.
}