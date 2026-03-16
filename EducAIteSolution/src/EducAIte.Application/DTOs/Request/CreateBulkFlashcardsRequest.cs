namespace EducAIte.Application.DTOs.Request;



public record CreateBulkFlashcardsRequest
{
    public required string Notesqid { get; init; } = string.Empty;

    public required IReadOnlyList<CreateBulkFlashcardItem> Flashcards {get; init;} = [];
}