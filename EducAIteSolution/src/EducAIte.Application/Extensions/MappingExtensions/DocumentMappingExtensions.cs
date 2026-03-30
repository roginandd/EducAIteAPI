using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class DocumentMappingExtensions
{
    public static DocumentResponse ToResponse(this Document document) => document.Adapt<DocumentResponse>();

    public static Document ToEntity(this CreateDocumentRequest request)
    {
        return new Document(
            request.DocumentName,
            request.FolderId,
            request.FileMetadataId
        );
    }

    public static void ApplyToEntity(this UpdateDocumentRequest request, Document document)
    {
        document.UpdateDetails(
            request.DocumentName,
            request.FolderId ?? document.FolderId,
            request.FileMetadataId ?? document.FileMetadataId
        );
    }
}
