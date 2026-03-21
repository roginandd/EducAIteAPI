using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Flashcard;

public sealed class FlashcardValidationException : ValidationException
{
    public FlashcardValidationException(string message)
        : base(message, "flashcard_validation_error")
    {
    }
}
