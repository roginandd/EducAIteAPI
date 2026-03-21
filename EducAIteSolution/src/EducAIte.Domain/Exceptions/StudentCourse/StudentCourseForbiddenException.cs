using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentCourse;

public sealed class StudentCourseForbiddenException : ForbiddenException
{
    public StudentCourseForbiddenException()
        : base("Student course does not belong to the authenticated student.", "student_course_forbidden")
    {
    }
}
