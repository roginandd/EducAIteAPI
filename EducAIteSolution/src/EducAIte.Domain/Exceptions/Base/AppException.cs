namespace EducAIte.Domain.Exceptions.Base;

public abstract class AppException : Exception
{
    public string ErrorCode { get; }

    protected AppException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
