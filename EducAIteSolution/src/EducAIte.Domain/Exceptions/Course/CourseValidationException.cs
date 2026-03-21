using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Course;

public sealed class CourseValidationException : ValidationException
{
    public CourseValidationException(string message)
        : base(message, "course_validation_error")
    {
    }
}
