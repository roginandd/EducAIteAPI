using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Document;

public sealed class DocumentValidationException : ValidationException
{
    public DocumentValidationException(string message)
        : base(message, "document_validation_error")
    {
    }
}
