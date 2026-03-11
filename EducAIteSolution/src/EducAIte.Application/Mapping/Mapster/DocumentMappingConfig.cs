namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

public sealed class DocumentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Document, DocumentResponse>();

        config.NewConfig<CreateDocumentRequest, Document>()
            .Ignore(destination => destination.DocumentId)
            .Ignore(destination => destination.Folder)
            .Ignore(destination => destination.FileMetadata)
            .Ignore(destination => destination.Notes)
            .Ignore(destination => destination.IsDeleted)
            .Ignore(destination => destination.CreatedAt)
            .Ignore(destination => destination.UpdatedAt);

        config.NewConfig<UpdateDocumentRequest, Document>()
            .Ignore(destination => destination.DocumentId)
            .Ignore(destination => destination.Folder)
            .Ignore(destination => destination.FileMetadata)
            .Ignore(destination => destination.Notes)
            .Ignore(destination => destination.IsDeleted)
            .Ignore(destination => destination.CreatedAt)
            .Ignore(destination => destination.UpdatedAt);
    }
}
