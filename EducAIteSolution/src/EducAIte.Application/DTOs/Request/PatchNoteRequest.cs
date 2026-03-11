namespace EducAIte.Application.DTOs.Request;

public record PatchNoteRequest
{
    public string? Name { get; init; }

    public string? NoteContent { get; init; }

    public long? DocumentId { get; init; }

    public long? SequenceNumber { get; init; }
}
