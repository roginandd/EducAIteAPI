namespace EducAIte.Application.DTOs.Request;

public record CreateNoteRequest
{
    public required string Name { get; init; } = string.Empty;

    public required string NoteContent { get; init; } = string.Empty;

    public required string DocumentSqid { get; init; } = string.Empty;
}
