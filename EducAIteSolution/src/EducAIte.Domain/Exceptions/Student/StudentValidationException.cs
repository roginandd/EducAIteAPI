namespace EducAIte.Domain.Exceptions.Student;

using EducAIte.Domain.Exceptions.Base;

/// <summary>
/// Represents validation errors for student operations.
/// </summary>
public sealed class StudentValidationException : ValidationException
{
    public StudentValidationException(string message)
        : base(message, "STUDENT_VALIDATION_ERROR")
    {
    }
}
