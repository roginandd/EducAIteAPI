namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

public sealed class NoteMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Note, NoteResponse>()
            .Map(
                destination => destination.DocumentExternalId,
                source => source.Document == null ? (Guid?)null : source.Document.ExternalId);
    }
}
