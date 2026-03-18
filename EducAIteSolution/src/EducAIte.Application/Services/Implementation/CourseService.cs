using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
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

    public async Task<BulkCreateCoursesResponse> CreateCoursesBulkAsync(
        CreateBulkCoursesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        int totalReceived = request.Courses.Count;
        if (totalReceived == 0)
        {
            return new BulkCreateCoursesResponse
            {
                TotalReceived = 0,
                DistinctAttempted = 0,
                InsertedCount = 0,
                SkippedCount = 0
            };
        }

        List<Course> distinctCourses = request.Courses
            .Where(course => !string.IsNullOrWhiteSpace(course.EDPCode))
            .GroupBy(course => course.EDPCode.Trim(), StringComparer.Ordinal)
            .Select(group =>
            {
                CreateBulkCourseItemRequest item = group.First();
                return new Course
                {
                    EDPCode = item.EDPCode.Trim(),
                    CourseName = item.CourseName?.Trim() ?? string.Empty,
                    Units = item.Units
                };
            })
            .ToList();

        if (distinctCourses.Count == 0)
        {
            return new BulkCreateCoursesResponse
            {
                TotalReceived = totalReceived,
                DistinctAttempted = 0,
                InsertedCount = 0,
                SkippedCount = totalReceived
            };
        }

        IReadOnlySet<string> existingEdpCodes = await _courseRepository.GetExistingEdpCodesAsync(
            distinctCourses.Select(course => course.EDPCode).ToArray(),
            cancellationToken);

        List<Course> missingCourses = distinctCourses
            .Where(course => !existingEdpCodes.Contains(course.EDPCode))
            .ToList();

        if (missingCourses.Count == 0)
        {
            return new BulkCreateCoursesResponse
            {
                TotalReceived = totalReceived,
                DistinctAttempted = distinctCourses.Count,
                InsertedCount = 0,
                SkippedCount = totalReceived
            };
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        int insertedCount;

        try
        {
            // Query existing EDP codes first so already-present rows do not consume sequence values
            // in the normal bulk-import path. ON CONFLICT remains as a final concurrency guard.
            insertedCount = await _courseRepository.InsertMissingCoursesAsync(missingCourses, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        int skippedCount = totalReceived - insertedCount;

        _logger.LogInformation(
            "Bulk inserted {InsertedCount} courses out of {DistinctAttempted} distinct items ({TotalReceived} received).",
            insertedCount,
            distinctCourses.Count,
            totalReceived);

        return new BulkCreateCoursesResponse
        {
            TotalReceived = totalReceived,
            DistinctAttempted = distinctCourses.Count,
            InsertedCount = insertedCount,
            SkippedCount = skippedCount
        };
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
