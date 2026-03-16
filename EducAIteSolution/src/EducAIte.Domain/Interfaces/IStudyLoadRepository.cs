using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

/// <summary>
/// Provides read access to study load ownership data needed by application services.
/// </summary>
public interface IStudyLoadRepository
{
    /// <summary>
    /// Retrieves an active study load by its identifier.
    /// </summary>
    Task<StudyLoad?> GetByIdAsync(long studyLoadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an active study load owned by a specific student.
    /// </summary>
    Task<StudyLoad?> GetByIdAndStudentIdAsync(long studyLoadId, long studentId, CancellationToken cancellationToken = default);
}
