using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.StudentFlashcard;

public sealed class StudentFlashcardNotFoundException : NotFoundException
{
    public StudentFlashcardNotFoundException(long flashcardId)
        : base($"Flashcard progress for flashcard ID {flashcardId} was not found.", "student_flashcard_not_found")
    {
    }
}
