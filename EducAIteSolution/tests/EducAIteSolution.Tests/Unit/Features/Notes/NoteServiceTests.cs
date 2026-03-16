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

namespace EducAIteSolution.Tests.Unit.Features.Notes;

public class NoteServiceTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<INoteOrderingService> _noteOrderingServiceMock;
    private readonly Mock<IResourceOwnershipService> _resourceOwnershipServiceMock;
    private readonly Mock<ILogger<NoteService>> _loggerMock;
    private readonly ISqidService _sqidService;
    private readonly NoteService _noteService;

    static NoteServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public NoteServiceTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _noteOrderingServiceMock = new Mock<INoteOrderingService>();
        _resourceOwnershipServiceMock = new Mock<IResourceOwnershipService>();
        _loggerMock = new Mock<ILogger<NoteService>>();
        _sqidService = new SqidService();

        _noteService = new NoteService(
            _noteRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _noteOrderingServiceMock.Object,
            _resourceOwnershipServiceMock.Object,
            _sqidService,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetNoteBySqidAsync_WithExistingNote_ReturnsSqids()
    {
        long noteId = 14;
        long documentId = 22;
        Note note = CreateNote(noteId, documentId, 7);

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureNoteOwnedByStudentAsync(noteId, 7, default))
            .Returns(Task.CompletedTask);
        _noteRepositoryMock
            .Setup(repository => repository.GetByIdAsync(noteId, default))
            .ReturnsAsync(note);

        NoteResponse? result = await _noteService.GetNoteBySqidAsync(_sqidService.Encode(noteId), 7);

        Assert.NotNull(result);
        Assert.Equal(_sqidService.Encode(noteId), result!.Sqid);
        Assert.Equal(_sqidService.Encode(documentId), result.DocumentSqid);
    }

    [Fact]
    public async Task GetNotesByDocumentAsync_WithExistingNotes_ReturnsSqids()
    {
        long documentId = 22;
        long noteId = 14;
        string documentSqid = _sqidService.Encode(documentId);
        Note note = CreateNote(noteId, documentId, 7);

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureDocumentOwnedByStudentAsync(documentId, 7, default))
            .Returns(Task.CompletedTask);
        _noteRepositoryMock
            .Setup(repository => repository.GetAllByDocumentIdAndStudentIdAsync(documentId, 7, default))
            .ReturnsAsync([note]);

        IReadOnlyList<NoteResponse> result = await _noteService.GetNotesByDocumentAsync(documentSqid, 7);

        Assert.Single(result);
        Assert.Equal(_sqidService.Encode(noteId), result[0].Sqid);
        Assert.Equal(documentSqid, result[0].DocumentSqid);
    }

    private static Note CreateNote(long noteId, long documentId, long studentId)
    {
        Note note = new("Linked Note", "Note content", documentId, 1m);
        SetPrivateProperty<Note, long>(note, nameof(Note.NoteId), noteId);
        SetPrivateProperty<Note, Document>(note, nameof(Note.Document), CreateDocument(documentId, 5, studentId));
        return note;
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
