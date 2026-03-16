using EducAIte.Application.DTOs.Request;

namespace EducAIte.Application.Services.Interface
{
    public interface IAWSService
    {
        Task<string> UploadNoteContextAsync(UploadNoteContextRequest uploadNoteContextRequest, CancellationToken cancellationToken);
        Task<string> UploadNoteImages(UploadNoteImagesRequest uploadNoteImagesRequest, CancellationToken cancellationToken);
        Task<string> UploadStudyLoad(StudyLoadCreateRequest studyLoadCreateRequest, CancellationToken cancellationToken);
        string GenerateNoteContextSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken);
        string GenerateNoteImageSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken);
        string GenerateStudyLoadSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken);
        string GenerateSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken);
    }
}