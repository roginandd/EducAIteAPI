using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

/// <summary>
/// Provides mapping helpers for grade responses.
/// </summary>
public static class GradeMappingExtensions
{
    /// <summary>
    /// Maps a grade entity to its API response.
    /// </summary>
    /// <param name="grade">The grade entity.</param>
    /// <param name="sqidService">The sqid encoder.</param>
    /// <returns>The mapped response DTO.</returns>
    public static GradeResponseDTO ToResponse(this Grade grade, ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(grade);
        ArgumentNullException.ThrowIfNull(sqidService);

        return grade
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<GradeResponseDTO>();
    }

    /// <summary>
    /// Maps a sequence of grade entities to API responses.
    /// </summary>
    /// <param name="grades">The grade entities.</param>
    /// <param name="sqidService">The sqid encoder.</param>
    /// <returns>The mapped response DTO collection.</returns>
    public static IReadOnlyList<GradeResponseDTO> ToResponses(this IEnumerable<Grade> grades, ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(grades);
        ArgumentNullException.ThrowIfNull(sqidService);

        return grades
            .Select(grade => grade.ToResponse(sqidService))
            .ToList();
    }
}
