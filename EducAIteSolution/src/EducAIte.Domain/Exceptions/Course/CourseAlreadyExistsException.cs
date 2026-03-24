using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Course;

public sealed class CourseAlreadyExistsException : ConflictException
{
    public CourseAlreadyExistsException(string edpCode)
        : base($"Course with EDP code {edpCode} already exists.", "course_already_exists")
    {
    }
}
