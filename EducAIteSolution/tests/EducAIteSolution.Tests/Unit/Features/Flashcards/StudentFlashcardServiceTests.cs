using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using Moq;

namespace EducAIteSolution.Tests.Unit.Features.Flashcards;

public class StudentFlashcardServiceTests
{
    private readonly Mock<IStudentFlashcardRepository> _studentFlashcardRepositoryMock;
    private readonly Mock<IFlashcardRepository> _flashcardRepositoryMock;
    private readonly Mock<ILogger<StudentFlashcardService>> _loggerMock;
    private readonly ISqidService _sqidService;
    private readonly StudentFlashcardService _studentFlashcardService;

    static StudentFlashcardServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public StudentFlashcardServiceTests()
    {
        _studentFlashcardRepositoryMock = new Mock<IStudentFlashcardRepository>();
        _flashcardRepositoryMock = new Mock<IFlashcardRepository>();
        _loggerMock = new Mock<ILogger<StudentFlashcardService>>();
        _sqidService = new SqidService();

        _studentFlashcardService = new StudentFlashcardService(
            _studentFlashcardRepositoryMock.Object,
            _flashcardRepositoryMock.Object,
            _sqidService,
            _loggerMock.Object);
    }

    [Fact]
    public async Task StartTrackingAsync_WithArchivedProgress_RestoresProgress()
    {
        long flashcardId = 14;
        Flashcard flashcard = CreateFlashcard(flashcardId, 22, 30, 7);
        StudentFlashcard archived = new(7, flashcardId);
        archived.MarkDeleted();

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync((StudentFlashcard?)null);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync(archived);

        StudentFlashcardProgressResponse result = await _studentFlashcardService.StartTrackingAsync(_sqidService.Encode(flashcardId), 7);

        Assert.Equal(0, result.TotalAttempts);
        Assert.Equal(_sqidService.Encode(flashcardId), result.FlashcardSqid);
        Assert.Equal(_sqidService.Encode(22), result.NoteSqid);
        Assert.Equal(_sqidService.Encode(30), result.DocumentSqid);
        Assert.False(archived.IsDeleted);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(archived, default), Times.Once);
    }

    [Fact]
    public async Task RecordCorrectAsync_WithoutExistingProgress_CreatesAndUpdatesProgress()
    {
        long flashcardId = 21;
        Flashcard flashcard = CreateFlashcard(flashcardId, 40, 50, 7);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync((StudentFlashcard?)null);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync((StudentFlashcard?)null);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<StudentFlashcard>(), default))
            .ReturnsAsync((StudentFlashcard progress, CancellationToken _) => progress);

        StudentFlashcardProgressResponse result = await _studentFlashcardService.RecordCorrectAsync(_sqidService.Encode(flashcardId), 7);

        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(1, result.TotalAttempts);
        _studentFlashcardRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<StudentFlashcard>(), default), Times.Once);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(It.Is<StudentFlashcard>(progress => progress.CorrectCount == 1), default), Times.Once);
    }

    [Fact]
    public async Task SubmitAttemptAsync_WithCorrectAnswer_ReturnsScheduledResult()
    {
        long flashcardId = 24;
        Flashcard flashcard = CreateFlashcard(flashcardId, 40, 50, 7, "Question", "Correct Answer");

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync((StudentFlashcard?)null);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync((StudentFlashcard?)null);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<StudentFlashcard>(), default))
            .ReturnsAsync((StudentFlashcard progress, CancellationToken _) => progress);

        FlashcardAttemptResultResponse result = await _studentFlashcardService.SubmitAttemptAsync(
            _sqidService.Encode(flashcardId),
            new SubmitFlashcardAttemptRequest { Answer = "  correct   answer " },
            7);

        Assert.True(result.IsCorrect);
        Assert.False(result.ShowAgainInSession);
        Assert.Equal(0, result.RequeueAfter);
        Assert.Equal(1, result.Progress.CorrectCount);
        Assert.Equal("Learning", result.Progress.ReviewState);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(It.Is<StudentFlashcard>(progress => progress.CorrectCount == 1), default), Times.Once);
    }

    [Fact]
    public async Task SubmitAttemptAsync_WithWrongAnswer_ReturnsLocalRequeueHint()
    {
        long flashcardId = 25;
        Flashcard flashcard = CreateFlashcard(flashcardId, 41, 51, 7, "Question", "Correct Answer");
        StudentFlashcard progress = new(7, flashcardId);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync(progress);

        FlashcardAttemptResultResponse result = await _studentFlashcardService.SubmitAttemptAsync(
            _sqidService.Encode(flashcardId),
            new SubmitFlashcardAttemptRequest { Answer = "Wrong" },
            7);

        Assert.False(result.IsCorrect);
        Assert.True(result.ShowAgainInSession);
        Assert.Equal(3, result.RequeueAfter);
        Assert.Equal(1, result.Progress.WrongCount);
        Assert.Equal(1, result.Progress.LapseCount);
        Assert.Equal("Learning", result.Progress.ReviewState);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(progress, default), Times.Once);
    }

    [Fact]
    public async Task ResetProgressAsync_WithExistingProgress_ResetsCounters()
    {
        long flashcardId = 33;
        Flashcard flashcard = CreateFlashcard(flashcardId, 45, 55, 7);
        StudentFlashcard progress = new(7, flashcardId);
        SetPrivateProperty(progress, nameof(StudentFlashcard.Flashcard), flashcard);
        progress.MarkCorrect();
        progress.MarkWrong();

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync(progress);

        StudentFlashcardProgressResponse result = await _studentFlashcardService.ResetProgressAsync(_sqidService.Encode(flashcardId), 7);

        Assert.Equal(0, result.CorrectCount);
        Assert.Equal(0, result.WrongCount);
        Assert.Equal(0, result.TotalAttempts);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(progress, default), Times.Once);
    }

    [Fact]
    public async Task GetReviewQueueAsync_ReturnsMappedReviewItems()
    {
        long firstFlashcardId = 60;
        long secondFlashcardId = 61;

        StudentFlashcard first = new(7, firstFlashcardId);
        SetPrivateProperty(first, nameof(StudentFlashcard.Flashcard), CreateFlashcard(firstFlashcardId, 90, 100, 7, "Question A", "Answer A"));
        first.MarkWrong();

        StudentFlashcard second = new(7, secondFlashcardId);
        SetPrivateProperty(second, nameof(StudentFlashcard.Flashcard), CreateFlashcard(secondFlashcardId, 91, 101, 7, "Question B", "Answer B"));
        second.MarkCorrect();

        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetReviewQueueByStudentIdAsync(7, default))
            .ReturnsAsync(new List<StudentFlashcard> { first, second });

        IReadOnlyList<FlashcardReviewItemResponse> result = await _studentFlashcardService.GetReviewQueueAsync(7);

        Assert.Equal(2, result.Count);
        Assert.Equal("Question A", result[0].Question);
        Assert.Equal(_sqidService.Encode(firstFlashcardId), result[0].FlashcardSqid);
        Assert.Equal(1, result[0].WrongCount);
    }

    [Fact]
    public async Task GetNextBatchAsync_WithDueAndUntrackedFlashcards_ReturnsMixedBatch()
    {
        long dueFlashcardId = 80;
        long newFlashcardId = 81;
        StudentFlashcard dueProgress = new(7, dueFlashcardId);
        SetPrivateProperty(dueProgress, nameof(StudentFlashcard.Flashcard), CreateFlashcard(dueFlashcardId, 90, 100, 7, "Due Question", "Due Answer"));
        dueProgress.MarkWrong();
        dueProgress.ResetProgress(DateTime.UtcNow.AddMinutes(-10));
        dueProgress.ApplyWrongReview(DateTime.UtcNow.AddMinutes(-6));

        Flashcard newFlashcard = CreateFlashcard(newFlashcardId, 91, 101, 7, "New Question", "New Answer");

        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetDueBatchByStudentIdAsync(7, It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<long>>(), 2, default))
            .ReturnsAsync(new List<StudentFlashcard> { dueProgress });
        _flashcardRepositoryMock
            .Setup(repository => repository.GetUntrackedByStudentIdAsync(7, It.IsAny<IReadOnlyCollection<long>>(), 1, default))
            .ReturnsAsync(new List<Flashcard> { newFlashcard });

        IReadOnlyList<FlashcardReviewItemResponse> result = await _studentFlashcardService.GetNextBatchAsync(
            7,
            new GetFlashcardReviewBatchRequest { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsTracked);
        Assert.False(result[1].IsTracked);
        Assert.Equal("Due Question", result[0].Question);
        Assert.Equal("New Question", result[1].Question);
    }

    [Fact]
    public async Task ArchiveProgressAsync_WithExistingProgress_SoftDeletesProgress()
    {
        long flashcardId = 75;
        Flashcard flashcard = CreateFlashcard(flashcardId, 101, 111, 7);
        StudentFlashcard progress = new(7, flashcardId);

        _flashcardRepositoryMock
            .Setup(repository => repository.GetByIdAndStudentIdAsync(flashcardId, 7, default))
            .ReturnsAsync(flashcard);
        _studentFlashcardRepositoryMock
            .Setup(repository => repository.GetTrackedByStudentAndFlashcardIdAsync(7, flashcardId, default))
            .ReturnsAsync(progress);

        await _studentFlashcardService.ArchiveProgressAsync(_sqidService.Encode(flashcardId), 7);

        Assert.True(progress.IsDeleted);
        _studentFlashcardRepositoryMock.Verify(repository => repository.UpdateAsync(progress, default), Times.Once);
    }

    private static Flashcard CreateFlashcard(
        long flashcardId,
        long noteId,
        long documentId,
        long studentId,
        string question = "Question",
        string answer = "Answer")
    {
        Document document = new("Linked Document", 3, 9)
        {
            Folder = new Folder
            {
                FolderId = 3,
                StudentId = studentId,
                CourseId = 5,
                SchoolYear = new EducAIte.Domain.ValueObjects.SchoolYear(2025, 2026),
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
        SetPrivateProperty(document, nameof(Document.DocumentId), documentId);

        Note note = new("Linked Note", "Note content", documentId, 1m);
        SetPrivateProperty(note, nameof(Note.NoteId), noteId);
        SetPrivateProperty(note, nameof(Note.Document), document);

        Flashcard flashcard = new(question, answer, noteId);
        SetPrivateProperty(flashcard, nameof(Flashcard.FlashcardId), flashcardId);
        SetPrivateProperty(flashcard, nameof(Flashcard.Note), note);
        return flashcard;
    }

    private static void SetPrivateProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
    {
        typeof(TTarget).GetProperty(propertyName)!.SetValue(target, value);
    }
}
