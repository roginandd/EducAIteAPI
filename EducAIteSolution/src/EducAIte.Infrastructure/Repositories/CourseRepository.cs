using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
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
}
