namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Mapster;

public sealed class FlashcardMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Flashcard, FlashcardResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.FlashcardId))
            .Map(dest => dest.NoteSqid, src => GetSqidService().Encode(src.NoteId))
            .Map(dest => dest.DocumentSqid, src => GetSqidService().Encode(src.Note.DocumentId));

        config.NewConfig<StudentFlashcard, StudentFlashcardProgressResponse>()
            .Map(dest => dest.FlashcardSqid, src => GetSqidService().Encode(GetFlashcard(src).FlashcardId))
            .Map(dest => dest.NoteSqid, src => GetSqidService().Encode(GetFlashcard(src).NoteId))
            .Map(dest => dest.DocumentSqid, src => GetSqidService().Encode(GetFlashcard(src).Note.DocumentId))
            .Map(dest => dest.ConsecutiveWrongCount, src => src.ConsecutiveWrongCount)
            .Map(dest => dest.ConsecutiveCorrectCount, src => src.ConsecutiveCorrectCount)
            .Map(dest => dest.ReviewState, src => src.State.ToString())
            .Map(dest => dest.LastReviewOutcome, src => src.LastReviewOutcome.HasValue ? src.LastReviewOutcome.Value.ToString() : null);

        config.NewConfig<StudentFlashcard, FlashcardReviewItemResponse>()
            .Map(dest => dest.FlashcardSqid, src => GetSqidService().Encode(GetFlashcard(src).FlashcardId))
            .Map(dest => dest.NoteSqid, src => GetSqidService().Encode(GetFlashcard(src).NoteId))
            .Map(dest => dest.DocumentSqid, src => GetSqidService().Encode(GetFlashcard(src).Note.DocumentId))
            .Map(dest => dest.Question, src => GetFlashcard(src).Question)
            .Map(dest => dest.IsTracked, src => true)
            .Map(dest => dest.ReviewState, src => src.State.ToString());

        config.NewConfig<Flashcard, FlashcardReviewItemResponse>()
            .Map(dest => dest.FlashcardSqid, src => GetSqidService().Encode(src.FlashcardId))
            .Map(dest => dest.NoteSqid, src => GetSqidService().Encode(src.NoteId))
            .Map(dest => dest.DocumentSqid, src => GetSqidService().Encode(src.Note.DocumentId))
            .Map(dest => dest.IsTracked, src => false)
            .Map(dest => dest.ReviewState, src => FlashcardReviewState.New.ToString())
            .Map(dest => dest.LastReviewedAt, src => (DateTime?)null)
            .Map(dest => dest.NextReviewAt, src => GetAvailableAt());


        config.NewConfig<CreateBulkFlashcardItem, Flashcard>()
            .Map(dest => dest.Question, src => src.Question)
            .Map(dest => dest.Answer, src => src.Answer);

        
            
    }

    private static ISqidService GetSqidService()
    {
        if (MapContext.Current?.Parameters.TryGetValue("sqidService", out object? value) != true || value is not ISqidService sqidService)
        {
            throw new InvalidOperationException("sqidService mapping parameter is required.");
        }

        return sqidService;
    }

    private static Flashcard GetFlashcard(StudentFlashcard studentFlashcard)
    {
        if (MapContext.Current?.Parameters.TryGetValue("flashcard", out object? value) == true && value is Flashcard flashcard)
        {
            return flashcard;
        }

        if (studentFlashcard.Flashcard is not null)
        {
            return studentFlashcard.Flashcard;
        }

        throw new InvalidOperationException("flashcard mapping parameter is required when StudentFlashcard.Flashcard is not loaded.");
    }

    private static DateTime GetAvailableAt()
    {
        if (MapContext.Current?.Parameters.TryGetValue("availableAt", out object? value) != true || value is not DateTime availableAt)
        {
            throw new InvalidOperationException("availableAt mapping parameter is required.");
        }

        return availableAt;
    }
}
