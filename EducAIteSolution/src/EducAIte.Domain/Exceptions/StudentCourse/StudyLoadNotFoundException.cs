using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentCourse;

public sealed class StudyLoadNotFoundException : NotFoundException
{
    public StudyLoadNotFoundException(long studyLoadId)
        : base($"Study load with ID {studyLoadId} was not found.", "study_load_not_found")
    {
    }
}
