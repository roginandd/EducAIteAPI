using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Domain.ValueObjects;
using Mapster;
using Microsoft.Extensions.Logging;
using Moq;

namespace EducAIteSolution.Tests.Unit.Features.Flashcards;

public class FlashcardServiceTests
{
    private readonly Mock<IFlashcardRepository> _flashcardRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IStudentFlashcardRepository> _studentFlashcardRepositoryMock;
    private readonly Mock<IResourceOwnershipService> _resourceOwnershipServiceMock;
    private readonly Mock<ILogger<FlashcardService>> _loggerMock;
    private readonly ISqidService _sqidService;
    private readonly FlashcardService _flashcardService;

    static FlashcardServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public FlashcardServiceTests()
    {
        _flashcardRepositoryMock = new Mock<IFlashcardRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _studentFlashcardRepositoryMock = new Mock<IStudentFlashcardRepository>();
        _resourceOwnershipServiceMock = new Mock<IResourceOwnershipService>();
        _loggerMock = new Mock<ILogger<FlashcardService>>();
        _sqidService = new SqidService();

        _flashcardService = new FlashcardService(
            _flashcardRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _studentFlashcardRepositoryMock.Object,
            _resourceOwnershipServiceMock.Object,
            _sqidService,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetBySqidAsync_WithInvalidSqid_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _flashcardService.GetBySqidAsync("not-a-valid-sqid", 7));
    }

    [Fact]
    public async Task GetBySqidAsync_WhenFlashcardBelongsToAnotherStudent_ThrowsUnauthorizedAccessException()
    {
        long flashcardId = 15;
        string flashcardSqid = _sqidService.Encode(flashcardId);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync((Flashcard?)null);
        _flashcardRepositoryMock
            .Setup(repository => repository.ExistsByIdAsync(flashcardId, default))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _flashcardService.GetBySqidAsync(flashcardSqid, 7));
    }

    [Fact]
    public async Task GetByDocumentAsync_WithValidDocument_ReturnsFlashcards()
    {
        long documentId = 12;
        string documentSqid = _sqidService.Encode(documentId);
        Flashcard flashcard = new("Question", "Answer", documentId);
        SetPrivateProperty<Flashcard, long>(flashcard, nameof(Flashcard.FlashcardId), 99L);

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureDocumentOwnedByStudentAsync(documentId, 7, default))
            .Returns(Task.CompletedTask);
        _flashcardRepositoryMock
            .Setup(repository => repository.GetAllByDocumentIdAndStudentIdAsync(documentId, 7, default))
            .ReturnsAsync([flashcard]);

        IReadOnlyList<FlashcardResponse> result = await _flashcardService.GetByDocumentAsync(documentSqid, 7);

        Assert.Single(result);
        Assert.Equal("Question", result[0].Question);
        Assert.Equal(documentSqid, result[0].DocumentSqid);
    }

    [Fact]
    public async Task GetByDocumentAsync_WithInvalidSqid_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _flashcardService.GetByDocumentAsync("not-a-valid-sqid", 7));
    }

    [Fact]
    public async Task CreateAsync_WithValidDocument_CreatesFlashcard()
    {
        long documentId = 22;

        CreateFlashcardRequest request = new()
        {
            Question = "What is polymorphism?",
            Answer = "Different implementations behind one contract.",
            DocumentSqid = _sqidService.Encode(documentId)
        };

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureDocumentOwnedByStudentAsync(documentId, 7, default))
            .Returns(Task.CompletedTask);
        _documentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(documentId, default))
            .ReturnsAsync(CreateDocument(documentId, 5, 7));
        _flashcardRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Flashcard>(), default))
            .ReturnsAsync((Flashcard flashcard, CancellationToken _) =>
            {
                SetPrivateProperty<Flashcard, long>(flashcard, nameof(Flashcard.FlashcardId), 51L);
                return flashcard;
            });

        FlashcardResponse created = await _flashcardService.CreateAsync(request, 7);

        Assert.Equal("What is polymorphism?", created.Question);
        Assert.Equal(request.DocumentSqid, created.DocumentSqid);
    }

    [Fact]
    public async Task UpdateAsync_WithValidDocument_UpdatesFlashcard()
    {
        long flashcardId = 31;
        long documentId = 22;

        UpdateFlashcardRequest request = new()
        {
            Question = "Updated question",
            Answer = "Updated answer",
            DocumentSqid = _sqidService.Encode(documentId)
        };

        Flashcard existing = new("Question", "Answer", 9);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(existing);
        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureDocumentOwnedByStudentAsync(documentId, 7, default))
            .Returns(Task.CompletedTask);
        _documentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(documentId, default))
            .ReturnsAsync(CreateDocument(documentId, 5, 7));

        bool updated = await _flashcardService.UpdateAsync(_sqidService.Encode(flashcardId), request, 7);

        Assert.True(updated);
        Assert.Equal("Updated question", existing.Question);
        Assert.Equal("Updated answer", existing.Answer);
        Assert.Equal(documentId, existing.DocumentId);
        _flashcardRepositoryMock.Verify(repository => repository.UpdateAsync(existing, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFlashcardIsMissing_ReturnsFalse()
    {
        long flashcardId = 41;

        _flashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync((Flashcard?)null);
        _flashcardRepositoryMock
            .Setup(repository => repository.ExistsByIdAsync(flashcardId, default))
            .ReturnsAsync(false);

        bool deleted = await _flashcardService.DeleteAsync(_sqidService.Encode(flashcardId), 7);

        Assert.False(deleted);
        _flashcardRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Flashcard>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithTrackedProgress_ArchivesProgressEntries()
    {
        long flashcardId = 52;
        Flashcard flashcard = new("Question", "Answer", 9);
        StudentFlashcard progress = new(7, flashcardId);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByFlashcardIdAsync(flashcardId, default))
            .ReturnsAsync([progress]);

        bool deleted = await _flashcardService.DeleteAsync(_sqidService.Encode(flashcardId), 7);

        Assert.True(deleted);
        Assert.True(flashcard.IsDeleted);
        Assert.True(progress.IsDeleted);
        _flashcardRepositoryMock.Verify(repository => repository.UpdateAsync(flashcard, default), Times.Once);
    }

    private static Document CreateDocument(long documentId, long courseId, long studentId)
    {
        Document document = new("Linked Document", 3, 9)
        {
            Folder = new Folder
            {
                FolderId = 3,
                StudentId = studentId,
                CourseId = courseId,
                SchoolYear = new SchoolYear(2025, 2026),
                FolderKey = "folder-3",
                Name = "Lecture Files"
            },
            FileMetadata = new FileMetadata
            {
                FileMetaDataId = 9,
                FileName = "notes.pdf",
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                StorageKey = "documents/notes.pdf",
                FileSizeInBytes = 1024,
                StudentId = studentId
            }
        };

        SetPrivateProperty<Document, long>(document, nameof(Document.DocumentId), documentId);
        return document;
    }

    private static void SetPrivateProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
    {
        typeof(TTarget).GetProperty(propertyName)!.SetValue(target, value);
    }
}
