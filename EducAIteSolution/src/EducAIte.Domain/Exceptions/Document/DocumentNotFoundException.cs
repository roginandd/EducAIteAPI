using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Document;

public sealed class DocumentNotFoundException : NotFoundException
{
    public DocumentNotFoundException(long documentId)
        : base($"Document with ID {documentId} was not found.", "document_not_found")
    {
    }
}
