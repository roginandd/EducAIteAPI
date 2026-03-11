using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Extensions;
using EducAIte.Application.Services.Implementation;
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
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _documentService;

    static DocumentServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public DocumentServiceTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _loggerMock = new Mock<ILogger<DocumentService>>();
        _documentService = new DocumentService(
            _documentRepositoryMock.Object,
            _studentRepositoryMock.Object,
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

        var result = await _documentService.CreateDocumentAsync(request);

        Assert.Equal("Midterm Notes", result.DocumentName);
        Assert.Equal(12, result.FolderId);
        Assert.Equal(33, result.FileMetadataId);
        Assert.Equal(7, result.StudentId);
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => _documentService.CreateDocumentAsync(request));

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

        var result = await _documentService.UpdateDocumentAsync(100, request);

        Assert.True(result);
        _documentRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Document>(), default), Times.Never);
    }
}
