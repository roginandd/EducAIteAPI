namespace EducAIte.Domain.Exceptions.Base;

public class NotFoundException : AppException
{
    public NotFoundException(string message, string errorCode = "not_found")
        : base(message, errorCode)
    {
    }
}
