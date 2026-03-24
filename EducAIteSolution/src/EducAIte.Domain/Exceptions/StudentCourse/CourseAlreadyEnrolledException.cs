using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentCourse;

public sealed class CourseAlreadyEnrolledException : ConflictException
{
    public CourseAlreadyEnrolledException()
        : base("The course is already enrolled for the selected study load.", "course_already_enrolled")
    {
    }
}
