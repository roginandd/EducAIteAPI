using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentFlashcard;

public sealed class StudentFlashcardValidationException : ValidationException
{
    public StudentFlashcardValidationException(string message)
        : base(message, "student_flashcard_validation_error")
    {
    }
}
