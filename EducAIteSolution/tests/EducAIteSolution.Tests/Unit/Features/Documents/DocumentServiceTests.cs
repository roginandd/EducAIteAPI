using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Extensions;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using Moq;

namespace EducAIteSolution.Tests.Unit.Features.Documents;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly Mock<IResourceOwnershipService> _resourceOwnershipServiceMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly ISqidService _sqidService;
    private readonly DocumentService _documentService;

    static DocumentServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public DocumentServiceTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _resourceOwnershipServiceMock = new Mock<IResourceOwnershipService>();
        _loggerMock = new Mock<ILogger<DocumentService>>();
        _sqidService = new SqidService();
        _documentService = new DocumentService(
            _documentRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _resourceOwnershipServiceMock.Object,
            _sqidService,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateDocumentAsync_WithValidOwnership_ReturnsCreatedDocument()
    {
        var request = new CreateDocumentRequest
        {
            DocumentName = " Midterm Notes ",
            FolderId = 12,
            FileMetadataId = 33
        };

        _documentRepositoryMock
            .Setup(repository => repository.GetFolderStudentIdAsync(request.FolderId, default))
            .ReturnsAsync(7);
        _documentRepositoryMock
            .Setup(repository => repository.GetFileMetadataStudentIdAsync(request.FileMetadataId, default))
            .ReturnsAsync(7);
        _documentRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Document>(), default))
            .ReturnsAsync((Document document, CancellationToken _) =>
            {
                SetPrivateProperty(document, nameof(Document.DocumentId), 101L);
                document.Folder = new Folder
                {
                    FolderId = document.FolderId,
                    StudentId = 7,
                    SchoolYear = new(2025, 2026),
                    FolderKey = "folder-12",
                    Name = "Semester Files"
                };
                document.FileMetadata = new FileMetadata
                {
                    FileMetaDataId = document.FileMetadataId,
                    FileName = "notes.pdf",
                    FileExtension = ".pdf",
                    ContentType = "application/pdf",
                    StorageKey = "docs/notes.pdf",
                    FileSizeInBytes = 1024,
                    StudentId = 7
                };

                return document;
            });

        var result = await _documentService.CreateDocumentAsync(request, 7);

        Assert.Equal("Midterm Notes", result.DocumentName);
        Assert.Equal(_sqidService.Encode(12), result.FolderSqid);
        Assert.Equal(_sqidService.Encode(33), result.FileMetadataSqid);
    }

    [Fact]
    public async Task CreateDocumentAsync_WithMismatchedOwnership_ThrowsInvalidOperationException()
    {
        var request = new CreateDocumentRequest
        {
            DocumentName = "Lecture Slides",
            FolderId = 10,
            FileMetadataId = 20
        };

        _documentRepositoryMock
            .Setup(repository => repository.GetFolderStudentIdAsync(request.FolderId, default))
            .ReturnsAsync(1);
        _documentRepositoryMock
            .Setup(repository => repository.GetFileMetadataStudentIdAsync(request.FileMetadataId, default))
            .ReturnsAsync(2);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _documentService.CreateDocumentAsync(request, 1));

        _documentRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Document>(), default), Times.Never);
    }

    [Fact]
    public async Task GetDocumentsByStudentAsync_WithMissingStudent_ThrowsKeyNotFoundException()
    {
        _studentRepositoryMock
            .Setup(repository => repository.GetByStudentIdAsync(15))
            .ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _documentService.GetDocumentsByStudentAsync(15));
    }

    [Fact]
    public async Task UpdateDocumentAsync_WithNoChanges_ReturnsTrueWithoutPersisting()
    {
        var request = new UpdateDocumentRequest
        {
            DocumentName = "Reference Guide",
            FolderId = 5,
            FileMetadataId = 9
        };

        _documentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(100, default))
            .ReturnsAsync(() =>
            {
                var document = new Document("Reference Guide", 5, 9)
                {
                    Folder = new Folder
                    {
                        FolderId = 5,
                        StudentId = 3,
                        SchoolYear = new(2025, 2026),
                        FolderKey = "folder-5",
                        Name = "Reviewers"
                    },
                    FileMetadata = new FileMetadata
                    {
                        FileMetaDataId = 9,
                        FileName = "review.pdf",
                        FileExtension = ".pdf",
                        ContentType = "application/pdf",
                        StorageKey = "docs/review.pdf",
                        FileSizeInBytes = 2048,
                        StudentId = 3
                    }
                };

                return document;
            });

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureDocumentOwnedByStudentAsync(100, 3, default))
            .Returns(Task.CompletedTask);

        var result = await _documentService.UpdateDocumentAsync(_sqidService.Encode(100), request, 3);

        Assert.True(result);
        _documentRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Document>(), default), Times.Never);
    }

    private static void SetPrivateProperty<T>(object target, string propertyName, T value)
    {
        typeof(Document).GetProperty(propertyName)!.SetValue(target, value);
    }
}
