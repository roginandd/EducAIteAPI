namespace EducAIte.Domain.Exceptions.Base;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message, string errorCode = "forbidden")
        : base(message, errorCode)
    {
    }
}
