using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Flashcard;

public sealed class FlashcardForbiddenException : ForbiddenException
{
    public FlashcardForbiddenException()
        : base("Flashcard does not belong to the authenticated student.", "flashcard_forbidden")
    {
    }
}
