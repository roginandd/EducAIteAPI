using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Course;

public sealed class CourseNotFoundException : NotFoundException
{
    public CourseNotFoundException(long courseId)
        : base($"Course with ID {courseId} was not found.", "course_not_found")
    {
    }
}
