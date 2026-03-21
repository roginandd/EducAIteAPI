namespace EducAIte.Domain.Exceptions.Base;

public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = "conflict")
        : base(message, errorCode)
    {
    }
}
