namespace EducAIte.Domain.Exceptions.Base;

public class ValidationException : AppException
{
    public ValidationException(string message, string errorCode = "validation_error")
        : base(message, errorCode)
    {
    }
}
