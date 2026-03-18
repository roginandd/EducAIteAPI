namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

/// <summary>
/// Registers FileMetadata mappings that depend on sqid conversion.
/// </summary>
public sealed class FileMetadataMappingConfig : IRegister
{
    /// <summary>
    /// Registers the file metadata mappings.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FileMetadata, FileMetadataResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.FileMetaDataId))
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId));
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
