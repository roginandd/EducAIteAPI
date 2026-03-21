using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Flashcard;

public sealed class FlashcardNotFoundException : NotFoundException
{
    public FlashcardNotFoundException(long flashcardId)
        : base($"Flashcard with ID {flashcardId} was not found.", "flashcard_not_found")
    {
    }
}
