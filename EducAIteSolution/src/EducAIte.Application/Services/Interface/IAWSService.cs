namespace EducAIte.Application.Services.Interface;

public interface IAWSService
{
    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken);
    string GenerateSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken);
}