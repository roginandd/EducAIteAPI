using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Document;

public sealed class DocumentForbiddenException : ForbiddenException
{
    public DocumentForbiddenException()
        : base("Document does not belong to the authenticated student.", "document_forbidden")
    {
    }
}
