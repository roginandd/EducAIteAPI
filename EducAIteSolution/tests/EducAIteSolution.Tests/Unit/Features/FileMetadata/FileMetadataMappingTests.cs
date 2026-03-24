using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIteSolution.Tests.Unit.Features.FileMetadata;

public class FileMetadataMappingTests
{
    private readonly ISqidService _sqidService = new SqidService();

    static FileMetadataMappingTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    [Fact]
    public void ToResponse_MapsExpectedFields()
    {
        var fileMetadata = new EducAIte.Domain.Entities.FileMetadata
        {
            FileMetaDataId = 15,
            StudentId = 7,
            FileName = "reviewer.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            StorageKey = "users/abc/reviewer.pdf",
            FileSizeInBytes = 4096,
            UploadedAt = new DateTime(2026, 3, 24, 1, 2, 3, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 24, 4, 5, 6, DateTimeKind.Utc)
        };

        FileMetadataResponse result = fileMetadata.ToResponse(_sqidService);

        Assert.Equal(15, result.FileMetadataId);
        Assert.Equal(_sqidService.Encode(15), result.Sqid);
        Assert.Equal(7, result.StudentId);
        Assert.Equal(_sqidService.Encode(7), result.StudentSqid);
        Assert.Equal("reviewer.pdf", result.FileName);
        Assert.Equal(".pdf", result.FileExtension);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("users/abc/reviewer.pdf", result.StorageKey);
        Assert.Equal(4096, result.FileSizeInBytes);
        Assert.Equal(new DateTime(2026, 3, 24, 1, 2, 3, DateTimeKind.Utc), result.UploadedAt);
        Assert.Equal(new DateTime(2026, 3, 24, 4, 5, 6, DateTimeKind.Utc), result.UpdatedAt);
    }
}
