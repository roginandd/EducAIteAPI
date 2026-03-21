namespace EducAIte.Domain.Exceptions.Student;

using EducAIte.Domain.Exceptions.Base;

/// <summary>
/// Represents errors that occur when creating a student that already exists.
/// </summary>
public sealed class StudentAlreadyExistsException : ConflictException
{
    public StudentAlreadyExistsException(string message)
        : base(message, "STUDENT_ALREADY_EXISTS")
    {
    }
}
