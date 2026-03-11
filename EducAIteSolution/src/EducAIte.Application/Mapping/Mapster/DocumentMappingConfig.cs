namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

public sealed class DocumentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Document, DocumentResponse>()
            .Map(destination => destination.StudentId, source => source.Folder.StudentId);

        config.NewConfig<CreateDocumentRequest, Document>()
            .Ignore(destination => destination.DocumentId)
            .Ignore(destination => destination.ExternalId)
            .Ignore(destination => destination.Folder)
            .Ignore(destination => destination.FileMetadata)
            .Ignore(destination => destination.Notes)
            .Ignore(destination => destination.IsDeleted)
            .Ignore(destination => destination.CreatedAt)
            .Ignore(destination => destination.UpdatedAt);

        config.NewConfig<UpdateDocumentRequest, Document>()
            .Ignore(destination => destination.DocumentId)
            .Ignore(destination => destination.ExternalId)
            .Ignore(destination => destination.Folder)
            .Ignore(destination => destination.FileMetadata)
            .Ignore(destination => destination.Notes)
            .Ignore(destination => destination.IsDeleted)
            .Ignore(destination => destination.CreatedAt)
            .Ignore(destination => destination.UpdatedAt);
    }
}
