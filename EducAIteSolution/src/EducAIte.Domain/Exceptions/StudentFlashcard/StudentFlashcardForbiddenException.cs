using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentFlashcard;

public sealed class StudentFlashcardForbiddenException : ForbiddenException
{
    public StudentFlashcardForbiddenException()
        : base("Flashcard progress does not belong to the authenticated student.", "student_flashcard_forbidden")
    {
    }
}
