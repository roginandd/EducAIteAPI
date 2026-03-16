using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class StudentFlashcardService : IStudentFlashcardService
{
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ISqidService _sqidService;
    private readonly ILogger<StudentFlashcardService> _logger;

    public StudentFlashcardService(
        IStudentFlashcardRepository studentFlashcardRepository,
        IFlashcardRepository flashcardRepository,
        ISqidService sqidService,
        ILogger<StudentFlashcardService> logger)
    {
        _studentFlashcardRepository = studentFlashcardRepository;
        _flashcardRepository = flashcardRepository;
        _sqidService = sqidService;
        _logger = logger;
    }

    public async Task<StudentFlashcardProgressResponse> StartTrackingAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));
        DateTime now = DateTime.UtcNow;

        Flashcard flashcard = await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? existing = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (existing is not null)
        {
            return existing.ToProgressResponse(_sqidService);
        }

        StudentFlashcard? archived = await _studentFlashcardRepository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (archived is not null)
        {
            archived.Restore(now);
            archived.StartTracking(now);
            await _studentFlashcardRepository.UpdateAsync(archived, cancellationToken);
            _logger.LogInformation("Restored flashcard progress for flashcard {FlashcardId}", flashcardId);
            return archived.ToProgressResponse(flashcard, _sqidService);
        }

        StudentFlashcard created = new(studentId, flashcardId);
        created.StartTracking(now);
        await _studentFlashcardRepository.AddAsync(created, cancellationToken);
        _logger.LogInformation("Started flashcard progress for flashcard {FlashcardId}", flashcardId);

        return created.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse?> GetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? studentFlashcard = await _studentFlashcardRepository.GetByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        return studentFlashcard?.ToProgressResponse(_sqidService);
    }

    public async Task<IReadOnlyList<FlashcardReviewItemResponse>> GetReviewQueueAsync(long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        IReadOnlyList<StudentFlashcard> reviewQueue = await _studentFlashcardRepository.GetReviewQueueByStudentIdAsync(studentId, cancellationToken);
        return reviewQueue
            .Select(studentFlashcard => studentFlashcard.ToReviewItemResponse(_sqidService))
            .ToList();
    }

    public async Task<IReadOnlyList<FlashcardReviewItemResponse>> GetNextBatchAsync(
        long studentId,
        GetFlashcardReviewBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        ArgumentNullException.ThrowIfNull(request);

        int take = NormalizeBatchSize(request.Take);
        DateTime now = DateTime.UtcNow;
        HashSet<long> excludedFlashcardIds = DecodeSqids(request.ExcludeFlashcardSqids ?? Array.Empty<string>());

        IReadOnlyList<StudentFlashcard> dueBatch = await _studentFlashcardRepository.GetDueBatchByStudentIdAsync(
            studentId,
            now,
            excludedFlashcardIds,
            take,
            cancellationToken);

        List<FlashcardReviewItemResponse> items = dueBatch
            .Select(studentFlashcard => studentFlashcard.ToReviewItemResponse(_sqidService))
            .ToList();

        if (items.Count == take)
        {
            return items;
        }

        foreach (StudentFlashcard dueItem in dueBatch)
        {
            excludedFlashcardIds.Add(dueItem.FlashcardId);
        }

        IReadOnlyList<Flashcard> newFlashcards = await _flashcardRepository.GetUntrackedByStudentIdAsync(
            studentId,
            excludedFlashcardIds,
            take - items.Count,
            cancellationToken);

        items.AddRange(newFlashcards.Select(flashcard => flashcard.ToReviewItemResponse(_sqidService, now)));
        return items;
    }

    public async Task<FlashcardAttemptResultResponse> SubmitAttemptAsync(
        string flashcardSqid,
        SubmitFlashcardAttemptRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);

        string submittedAnswer = request.Answer?.Trim() ?? string.Empty;
        bool isCorrect = AnswersMatch(submittedAnswer, flashcard.Answer);

        studentFlashcard.ApplyReviewResult(isCorrect, DateTime.UtcNow);
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);

        return new FlashcardAttemptResultResponse
        {
            FlashcardSqid = flashcardSqid,
            SubmittedAnswer = submittedAnswer,
            ExpectedAnswer = flashcard.Answer,
            Feedback = isCorrect
                ? "Correct. The flashcard will come back later."
                : "Incorrect. Review the expected answer and try it again later in the session.",
            IsCorrect = isCorrect,
            ShowAgainInSession = !isCorrect,
            RequeueAfter = isCorrect ? 0 : 3,
            NextReviewAt = studentFlashcard.NextReviewAt,
            Progress = studentFlashcard.ToProgressResponse(flashcard, _sqidService)
        };
    }

    public async Task<StudentFlashcardProgressResponse> RecordCorrectAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.MarkCorrect();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse> RecordWrongAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.MarkWrong();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task<StudentFlashcardProgressResponse> ResetProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        (StudentFlashcard studentFlashcard, Flashcard flashcard) = await EnsureTrackedProgressAsync(flashcardSqid, studentId, cancellationToken);
        studentFlashcard.ResetProgress();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        return studentFlashcard.ToProgressResponse(flashcard, _sqidService);
    }

    public async Task ArchiveProgressAsync(string flashcardSqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? studentFlashcard = await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (studentFlashcard is null)
        {
            return;
        }

        studentFlashcard.MarkDeleted();
        await _studentFlashcardRepository.UpdateAsync(studentFlashcard, cancellationToken);
        _logger.LogInformation("Archived flashcard progress for flashcard {FlashcardId}", flashcardId);
    }

    private async Task<(StudentFlashcard StudentFlashcard, Flashcard Flashcard)> EnsureTrackedProgressAsync(
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        EnsureStudentIdIsValid(studentId);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        Flashcard flashcard = await GetOwnedFlashcardOrThrowAsync(flashcardId, studentId, cancellationToken);
        StudentFlashcard? active = await _studentFlashcardRepository.GetTrackedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (active is not null)
        {
            return (active, flashcard);
        }

        StudentFlashcard? archived = await _studentFlashcardRepository.GetTrackedIncludingDeletedByStudentAndFlashcardIdAsync(studentId, flashcardId, cancellationToken);
        if (archived is not null)
        {
            archived.Restore(DateTime.UtcNow);
            await _studentFlashcardRepository.UpdateAsync(archived, cancellationToken);
            return (archived, flashcard);
        }

        StudentFlashcard created = new(studentId, flashcardId);
        created.StartTracking(DateTime.UtcNow);
        await _studentFlashcardRepository.AddAsync(created, cancellationToken);
        return (created, flashcard);
    }

    private async Task<Flashcard> GetOwnedFlashcardOrThrowAsync(long flashcardId, long studentId, CancellationToken cancellationToken)
    {
        Flashcard? flashcard = await _flashcardRepository.GetByIdAndStudentIdAsync(flashcardId, studentId, cancellationToken);
        if (flashcard is not null)
        {
            return flashcard;
        }

        bool exists = await _flashcardRepository.ExistsByIdAsync(flashcardId, cancellationToken);
        if (exists)
        {
            throw new UnauthorizedAccessException("Flashcard does not belong to the authenticated student.");
        }

        throw new KeyNotFoundException($"Flashcard with ID {flashcardId} was not found.");
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new ArgumentException($"{fieldName} is invalid.");
        }

        return id;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }

    private HashSet<long> DecodeSqids(IReadOnlyCollection<string> flashcardSqids)
    {
        HashSet<long> decoded = new();

        foreach (string flashcardSqid in flashcardSqids)
        {
            if (string.IsNullOrWhiteSpace(flashcardSqid))
            {
                continue;
            }

            decoded.Add(DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid)));
        }

        return decoded;
    }

    private static int NormalizeBatchSize(int take)
    {
        if (take <= 0)
        {
            return 30;
        }

        return Math.Min(take, 100);
    }

    private static bool AnswersMatch(string submittedAnswer, string expectedAnswer)
    {
        return NormalizeForComparison(submittedAnswer) == NormalizeForComparison(expectedAnswer);
    }

    private static string NormalizeForComparison(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            value.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToUpperInvariant();
    }
}
