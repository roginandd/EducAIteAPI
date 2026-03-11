namespace EducAIte.Application.DTOs.Request;

public record MoveNoteRequest
{
    public string? PreviousNoteSqid { get; init; }

    public string? NextNoteSqid { get; init; }
}

