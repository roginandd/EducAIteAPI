using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class StudentFlashcardMappingExtensions
{
    public static StudentFlashcardProgressResponse ToProgressResponse(
        this StudentFlashcard studentFlashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return studentFlashcard
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudentFlashcardProgressResponse>();
    }

    public static StudentFlashcardProgressResponse ToProgressResponse(
        this StudentFlashcard studentFlashcard,
        Flashcard flashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(flashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return studentFlashcard
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AddParameters("flashcard", flashcard)
            .AdaptToType<StudentFlashcardProgressResponse>();
    }

    public static FlashcardReviewItemResponse ToReviewItemResponse(
        this StudentFlashcard studentFlashcard,
        ISqidService sqidService)
    {
        ArgumentNullException.ThrowIfNull(studentFlashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return studentFlashcard
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<FlashcardReviewItemResponse>();
    }

    public static FlashcardReviewItemResponse ToReviewItemResponse(
        this Flashcard flashcard,
        ISqidService sqidService,
        DateTime availableAt)
    {
        ArgumentNullException.ThrowIfNull(flashcard);
        ArgumentNullException.ThrowIfNull(sqidService);

        return flashcard
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AddParameters("availableAt", availableAt)
            .AdaptToType<FlashcardReviewItemResponse>();
    }
}
