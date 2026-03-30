namespace EducAIte.Domain.Exceptions.Student;

using EducAIte.Domain.Exceptions.Base;

/// <summary>
/// Represents errors that occur when registering a student with an email that already exists.
/// </summary>
public sealed class StudentEmailAlreadyExistsException : ConflictException
{
    public StudentEmailAlreadyExistsException(string message)
        : base(message, "STUDENT_EMAIL_ALREADY_EXISTS")
    {
    }
}
