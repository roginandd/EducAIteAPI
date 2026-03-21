using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentCourse;

public sealed class StudentCourseNotFoundException : NotFoundException
{
    public StudentCourseNotFoundException(string studentCourseSqid)
        : base($"Student course '{studentCourseSqid}' was not found.", "student_course_not_found")
    {
    }
}
