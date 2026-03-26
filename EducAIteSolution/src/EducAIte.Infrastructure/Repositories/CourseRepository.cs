using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using EducAIte.Infrastructure.CustomQueries;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;


public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _dbContext;


    public CourseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Course?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {

        return await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Course?> GetByEDPCodeAsync(string edpCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EDPCode == edpCode && !c.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Courses
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return course;
    }

    public async Task<IReadOnlySet<string>> GetExistingEdpCodesAsync(
        IReadOnlyCollection<string> edpCodes,
        CancellationToken cancellationToken = default)
    {
        if (edpCodes.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        const int batchSize = 1000;
        HashSet<string> existingEdpCodes = new(StringComparer.Ordinal);

        foreach (string[] batch in edpCodes.Chunk(batchSize))
        {
            List<string> existingBatch = await _dbContext.Courses
                .Where(course => !course.IsDeleted && batch.Contains(course.EDPCode))
                .Select(course => course.EDPCode)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            existingEdpCodes.UnionWith(existingBatch);
        }

        return existingEdpCodes;
    }

    public async Task<IReadOnlyList<Course>> GetByEdpCodesAsync(
        IReadOnlyCollection<string> edpCodes,
        CancellationToken cancellationToken = default)
    {
        if (edpCodes.Count == 0)
        {
            return [];
        }

        return await _dbContext.Courses
            .Where(course => !course.IsDeleted && edpCodes.Contains(course.EDPCode))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> InsertMissingCoursesAsync(IReadOnlyList<Course> courses, CancellationToken cancellationToken = default)
    {
        if (courses.Count == 0)
        {
            return 0;
        }

        const int batchSize = 1000;
        int insertedCount = 0;

        foreach (Course[] batch in courses.Chunk(batchSize))
        {
            insertedCount += await ExecuteInsertBatchAsync(batch, cancellationToken);
        }

        return insertedCount;
    }

    public async Task UpdateAsync(long id, Course course, CancellationToken cancellationToken = default)
    {
        Course? existingCourse = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == id && !c.IsDeleted, cancellationToken);

        if (existingCourse is null)
        {
            return;
        }

        existingCourse.CourseName = course.CourseName;
        existingCourse.Units = course.Units;
        existingCourse.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        Course? course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == id && !c.IsDeleted, cancellationToken);
       
        if (course is null)
        {
            return;
        }

        course.IsDeleted = true;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> ExecuteInsertBatchAsync(IReadOnlyList<Course> batch, CancellationToken cancellationToken)
    {
        (string sql, Npgsql.NpgsqlParameter[] parameters) = CourseCustomQueries.BuildInsertMissingCourses(batch);
        return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}
