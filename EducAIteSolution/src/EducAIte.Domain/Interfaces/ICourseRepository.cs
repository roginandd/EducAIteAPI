using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

/// <summary>
/// Defines the persistence contract for the Course domain.
/// Responsible for low-level data access operations.
/// </summary>
public interface ICourseRepository
{
    /// <summary>
    /// Retrieves a course by its unique surrogate primary key.
    /// </summary>
    /// <param name="id">The internal database ID of the course.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The course entity if found; otherwise, null.</returns>
    Task<Course?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a course by its unique natural key (EDP Code).
    /// </summary>
    /// <param name="edpCode">The unique electronic data processing code of the course.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The course entity if found; otherwise, null.</returns>
    Task<Course?> GetByEDPCodeAsync(string edpCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all non-deleted courses from the system.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A collection of read-only course entities.</returns>
    Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new course entity to the database.
    /// </summary>
    /// <param name="course">The course entity to be created.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The created course entity with its generated identity.</returns>
    Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the subset of EDP codes that already exist in the database.
    /// </summary>
    /// <param name="edpCodes">The EDP codes to check.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The existing EDP codes.</returns>
    Task<IReadOnlySet<string>> GetExistingEdpCodesAsync(
        IReadOnlyCollection<string> edpCodes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists only the course rows whose EDP codes are missing from the database.
    /// </summary>
    /// <param name="courses">The course entities to insert.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The number of rows inserted.</returns>
    Task<int> InsertMissingCoursesAsync(IReadOnlyList<Course> courses, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the attributes of an existing course entity.
    /// </summary>
    /// <param name="course">The course entity containing updated values.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    Task UpdateAsync(long id, Course course, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a soft delete on a course by setting the IsDeleted flag.
    /// </summary>
    /// <param name="id">The internal database ID of the course to delete.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
