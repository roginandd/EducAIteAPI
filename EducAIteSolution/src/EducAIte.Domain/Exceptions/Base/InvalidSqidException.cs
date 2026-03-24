namespace EducAIte.Domain.Exceptions.Base;

public class InvalidSqidException : ValidationException
{
    public InvalidSqidException(string fieldName)
        : base($"{fieldName} is invalid.", "invalid_sqid")
    {
    }
}
