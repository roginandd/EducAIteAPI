using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class FileMetadataMappingExtensions
{
    public static FileMetadataResponse ToResponse(this FileMetadata fileMetadata, ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(fileMetadata);
        ArgumentNullException.ThrowIfNull(sqidService);

        return fileMetadata
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<FileMetadataResponse>();
    }

    
}
