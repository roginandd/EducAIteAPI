namespace EducAIte.Application.DTOs.Response;

public record SignedUrlResponse
{
    public string Url { get; init; } = string.Empty;
}
