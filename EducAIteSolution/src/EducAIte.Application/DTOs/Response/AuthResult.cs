namespace EducAIte.Application.DTOs.Response;


public record AuthResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }      // Nullable: Empty if login fails
    public DateTime? Expiration { get; init; } // Nullable: Empty if login fails
    public string? Error { get; init; }      // Nullable: Empty if login succeeds

    // 1. Helper for Success
    public static AuthResult Ok(string token, DateTime expiration)
    {
        return new AuthResult 
        { 
            Success = true, 
            Token = token, 
            Expiration = expiration 
        };
    }

    // 2. Helper for Failure
    public static AuthResult Fail(string error)
    {
        return new AuthResult 
        { 
            Success = false, 
            Error = error 
        };
    }
}