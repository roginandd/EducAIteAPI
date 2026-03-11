namespace EducAIte.Application.DTOs.Response;

public record NoteResponse
{
    public Guid ExternalId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NoteContent { get; init; } = string.Empty;
    
    public Guid? DocumentExternalId { get; init; }

    public long SequenceNumber { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
