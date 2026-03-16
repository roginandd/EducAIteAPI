namespace EducAIte.Application.DTOs.Request
{
    public class UploadNoteContextRequest
    {
        public string UserSqid { get; init; }
        public string NoteSqid { get; init; }
        public IFormFile File { get; init; }
    }

    public class UploadNoteImagesRequest
    {
        public string UserSqid { get; init; }

        public string NoteSqid { get; init; }
        public IFormFile File { get; init; }
    }
}