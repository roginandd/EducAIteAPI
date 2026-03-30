namespace EducAIte.Application.DTOs.Request;


public record CreateBulkFlashcardItem
{
    public required string Question { get; init; } = string.Empty;

    public required string Answer { get; init; } = string.Empty;

    public string ConceptExplanation { get; init; } = string.Empty;

    public string AnsweringGuidance { get; init; } = string.Empty;

    public IReadOnlyList<string> AcceptedAnswerAliases { get; init; } = [];
}
