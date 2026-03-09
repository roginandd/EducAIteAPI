using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<CourseResponse?> GetCourseByIdAsync(long id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        return course?.ToResponse();
    }

    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _courseRepository.GetAllAsync(cancellationToken);
        return courses.Select(course => course.ToResponse());
    }

    public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request)
    {
        var existingCourse = await _courseRepository.GetByEDPCodeAsync(request.EDPCode);
        if (existingCourse is not null)
        {
            throw new InvalidOperationException($"Course with EDP code {request.EDPCode} already exists.");
        }

        var newCourse = request.ToEntity();
        var createdCourse = await _courseRepository.AddAsync(newCourse);
        return createdCourse.ToResponse();
    }

    public async Task<bool> UpdateCourseAsync(long id, UpdateCourseRequest request)
    {
        var existingCourse = await _courseRepository.GetByIdAsync(id);
        if (existingCourse is null)
        {
            return false;
        }

        if (!string.Equals(existingCourse.EDPCode, request.EDPCode, StringComparison.OrdinalIgnoreCase))
        {
            var existingByEdp = await _courseRepository.GetByEDPCodeAsync(request.EDPCode);
            if (existingByEdp is not null && existingByEdp.CourseId != id)
            {
                throw new InvalidOperationException($"Course with EDP code {request.EDPCode} already exists.");
            }
        }

        if (existingCourse.CourseName == request.CourseName &&
            existingCourse.Units == request.Units &&
            string.Equals(existingCourse.EDPCode, request.EDPCode, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var updatedCourse = request.ToEntity(existingCourse);
        await _courseRepository.UpdateAsync(id, updatedCourse);

        _logger.LogInformation("Updated course {CourseId}", id);
        return true;
    }

    public async Task<bool> DeleteCourseAsync(long id)
    {
        var existingCourse = await _courseRepository.GetByIdAsync(id);
        if (existingCourse is null)
        {
            return false;
        }

        await _courseRepository.DeleteAsync(id);
        _logger.LogInformation("Deleted course {CourseId}", id);
        return true;
    }
}
