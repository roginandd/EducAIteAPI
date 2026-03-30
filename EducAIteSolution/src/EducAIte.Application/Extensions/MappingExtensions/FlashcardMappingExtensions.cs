using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;

namespace EducAIte.Application.Extensions.MappingExtensions;

using Mapster;

public static class FlashcardMappingExtensions
{
    public static FlashcardResponse ToResponse(this Flashcard flashcard, ISqidService sqidService)
    {
        return flashcard
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<FlashcardResponse>();
    }

    public static Flashcard ToEntity(
        this CreateFlashcardRequest request,
        long noteId)
    {
        return new Flashcard(
            request.Question,
            request.Answer,
            noteId,
            request.ConceptExplanation,
            request.AnsweringGuidance,
            request.AcceptedAnswerAliases);
    }

    public static void UpdateFromEntity(
        this UpdateFlashcardRequest request,
        Flashcard flashcard)
    {
        flashcard.UpdateContent(
            request.Question,
            request.Answer,
            request.ConceptExplanation,
            request.AnsweringGuidance,
            request.AcceptedAnswerAliases);
    }

    public static Flashcard ToEntity(
        this CreateBulkFlashcardItem item,
        long noteId
    )
    {
        return new Flashcard(
            item.Question,
            item.Answer,
            noteId,
            item.ConceptExplanation,
            item.AnsweringGuidance,
            item.AcceptedAnswerAliases);
    }
}
