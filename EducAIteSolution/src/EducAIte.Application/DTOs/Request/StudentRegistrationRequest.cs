public record StudentRegistrationRequest
{
    public required string FirstName { get; init; } = string.Empty;

    public string? MiddleName { get; init; } // Optional
    public required string LastName { get; init; } = string.Empty;
    public required string Email { get; init; } = string.Empty;
    public required string Password { get; init; } = string.Empty;
    
    public required string ConfirmPassword { get; init;} = string.Empty;
    
    public required string StudentIdNumber { get; init; } = string.Empty;
}