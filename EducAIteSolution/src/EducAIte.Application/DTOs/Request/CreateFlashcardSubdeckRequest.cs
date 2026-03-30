namespace EducAIte.Application.DTOs.Request;

public record CreateFlashcardSubdeckRequest
{
    public required string Name { get; init; } = string.Empty;

    public string InitialNoteContent { get; init; } = string.Empty;
}
