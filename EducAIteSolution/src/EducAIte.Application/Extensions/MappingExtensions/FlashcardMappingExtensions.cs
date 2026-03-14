using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

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
        long documentId)
    {
        return new Flashcard(
            request.Question,
            request.Answer,
            documentId);
    }

    public static void UpdateFromEntity(
        this UpdateFlashcardRequest request,
        Flashcard flashcard)
    {
        flashcard.UpdateContent(request.Question, request.Answer);
    }
}
