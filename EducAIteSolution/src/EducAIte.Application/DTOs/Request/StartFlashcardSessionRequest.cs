using EducAIte.Domain.Enum;

namespace EducAIte.Application.DTOs.Request;

public record StartFlashcardSessionRequest
{
    public FlashcardSessionScopeType ScopeType { get; init; } = FlashcardSessionScopeType.Course;

    public string? StudentCourseSqid { get; init; }

    public string? DocumentSqid { get; init; }

    public int Take { get; init; } = 30;
}
