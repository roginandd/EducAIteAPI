namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

public sealed class NoteMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Note, NoteResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.NoteId))
            .Map(dest => dest.DocumentSqid, src => GetSqidService().Encode(src.DocumentId));
    }

    private static ISqidService GetSqidService()
    {
        if (MapContext.Current?.Parameters.TryGetValue("sqidService", out object? value) != true || value is not ISqidService sqidService)
        {
            throw new InvalidOperationException("sqidService mapping parameter is required.");
        }

        return sqidService;
    }
}
