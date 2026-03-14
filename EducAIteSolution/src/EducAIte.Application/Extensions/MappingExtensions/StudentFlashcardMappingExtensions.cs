using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class StudentFlashcardMappingExtensions
{
    public static StudentFlashcardProgressResponse ToProgressResponse(
        this StudentFlashcard studentFlashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return studentFlashcard.ToProgressResponse(studentFlashcard.Flashcard, sqidService);
    }

    public static StudentFlashcardProgressResponse ToProgressResponse(
        this StudentFlashcard studentFlashcard,
        Flashcard flashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(flashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return new StudentFlashcardProgressResponse
        {
            FlashcardSqid = sqidService.Encode(flashcard.FlashcardId),
            DocumentSqid = sqidService.Encode(flashcard.DocumentId),
            CorrectCount = studentFlashcard.CorrectCount,
            WrongCount = studentFlashcard.WrongCount,
            TotalAttempts = studentFlashcard.TotalAttempts,
            ConsecutiveCorrectCount = studentFlashcard.ConsecutiveCorrectCount,
            ReviewCount = studentFlashcard.ReviewCount,
            LapseCount = studentFlashcard.LapseCount,
            ReviewState = studentFlashcard.State.ToString(),
            LastReviewOutcome = studentFlashcard.LastReviewOutcome?.ToString(),
            LastReviewedAt = studentFlashcard.LastReviewedAt,
            NextReviewAt = studentFlashcard.NextReviewAt,
            CreatedAt = studentFlashcard.CreatedAt,
            UpdatedAt = studentFlashcard.UpdatedAt
        };
    }

    public static FlashcardReviewItemResponse ToReviewItemResponse(
        this StudentFlashcard studentFlashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        Flashcard flashcard = studentFlashcard.Flashcard;

        return new FlashcardReviewItemResponse
        {
            FlashcardSqid = sqidService.Encode(flashcard.FlashcardId),
            DocumentSqid = sqidService.Encode(flashcard.DocumentId),
            Question = flashcard.Question,
            CorrectCount = studentFlashcard.CorrectCount,
            WrongCount = studentFlashcard.WrongCount,
            TotalAttempts = studentFlashcard.TotalAttempts,
            IsTracked = true,
            ReviewState = studentFlashcard.State.ToString(),
            LastReviewedAt = studentFlashcard.LastReviewedAt,
            NextReviewAt = studentFlashcard.NextReviewAt
        };
    }

    public static FlashcardReviewItemResponse ToReviewItemResponse(
        this Flashcard flashcard,
        ISqidService sqidService,
        DateTime availableAt)
    {
        ArgumentNullException.ThrowIfNull(flashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return new FlashcardReviewItemResponse
        {
            FlashcardSqid = sqidService.Encode(flashcard.FlashcardId),
            DocumentSqid = sqidService.Encode(flashcard.DocumentId),
            Question = flashcard.Question,
            CorrectCount = 0,
            WrongCount = 0,
            TotalAttempts = 0,
            IsTracked = false,
            ReviewState = FlashcardReviewState.New.ToString(),
            LastReviewedAt = null,
            NextReviewAt = availableAt
        };
    }
}
