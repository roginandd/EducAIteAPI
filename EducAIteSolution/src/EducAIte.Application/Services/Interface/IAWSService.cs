namespace EducAIte.Application.Services.Interface;

public interface IAWSService
{
    Task<string> UploadFileAsync(IFormFile file);
    string GenerateSignedUrl(string key, TimeSpan validFor);
}