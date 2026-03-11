namespace EducAIte.Application.DTOs.Response;

public record NoteResponse
{
    public string Sqid { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string NoteContent { get; init; } = string.Empty;
    
    public string DocumentSqid { get; init; } = string.Empty;

    public decimal SequenceNumber { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
