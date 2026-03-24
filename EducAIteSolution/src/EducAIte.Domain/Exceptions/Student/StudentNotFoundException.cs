namespace EducAIte.Domain.Exceptions.Student;

using EducAIte.Domain.Exceptions.Base;

/// <summary>
/// Represents errors that occur when a student cannot be found.
/// </summary>
public sealed class StudentNotFoundException : NotFoundException
{
    public StudentNotFoundException(string message)
        : base(message, "STUDENT_NOT_FOUND")
    {
    }
}
