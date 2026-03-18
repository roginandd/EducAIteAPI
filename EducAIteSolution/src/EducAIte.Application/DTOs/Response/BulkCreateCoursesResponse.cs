namespace EducAIte.Application.DTOs.Response;

/// <summary>
/// Represents the result of a bulk course insert operation.
/// </summary>
public record BulkCreateCoursesResponse
{
    /// <summary>
    /// Gets the number of items received in the request.
    /// </summary>
    public required int TotalReceived { get; init; }

    /// <summary>
    /// Gets the number of distinct course records attempted after request deduplication.
    /// </summary>
    public required int DistinctAttempted { get; init; }

    /// <summary>
    /// Gets the number of rows actually inserted.
    /// </summary>
    public required int InsertedCount { get; init; }

    /// <summary>
    /// Gets the number of rows skipped because they already existed, were duplicated in the request,
    /// or were filtered out before insertion.
    /// </summary>
    public required int SkippedCount { get; init; }
}
