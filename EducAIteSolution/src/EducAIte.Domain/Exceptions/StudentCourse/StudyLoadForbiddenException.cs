using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentCourse;

public sealed class StudyLoadForbiddenException : ForbiddenException
{
    public StudyLoadForbiddenException()
        : base("Study load does not belong to the authenticated student.", "study_load_forbidden")
    {
    }
}
