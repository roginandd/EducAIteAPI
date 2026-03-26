namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents a parsed study load payload that should be persisted for an existing study load.
/// </summary>
public record ApplyParsedStudyLoadCoursesRequest
{
    /// <summary>
    /// Gets the parsed course rows to attach to the study load.
    /// </summary>
    public required IReadOnlyList<CreateBulkCourseItemRequest> Courses { get; init; } = [];
}
