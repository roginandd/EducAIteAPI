using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Grade;

public sealed class InvalidGradeTypeException : ValidationException
{
    public InvalidGradeTypeException()
        : base("Grade type is invalid.", "invalid_grade_type")
    {
    }
}
