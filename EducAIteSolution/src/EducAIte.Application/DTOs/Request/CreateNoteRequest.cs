namespace EducAIte.Application.DTOs.Request;

public record CreateNoteRequest
{
    public required string Name { get; init; } = string.Empty;

    public required string NoteContent { get; init; } = string.Empty;

    public long DocumentId { get; init; }

    public long SequenceNumber { get; init; }
}
