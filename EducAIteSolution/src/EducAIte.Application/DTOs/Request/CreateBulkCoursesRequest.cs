namespace EducAIte.Application.DTOs.Request;

/// <summary>
/// Represents a bulk course creation request.
/// </summary>
public record CreateBulkCoursesRequest
{
    /// <summary>
    /// Gets the child course items to create.
    /// </summary>
    public required IReadOnlyList<CreateBulkCourseItemRequest> Courses { get; init; } = [];
}
