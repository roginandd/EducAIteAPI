namespace EducAIte.Domain.Entities;

public class StudentFlashcard
{
    // Surrogate Primary Key
    public long StudentFlashcardId { get; set;}

    // The link to the Student (Who is studying?)
    public long StudentId { get; set; }
    public Student Student { get; set; } = null!;

    // The link to the Flashcard (What are they studying?)
    public long FlashcardId { get; set; }
    public Flashcard Flashcard { get; set; } = null!;

    // Performance Data
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }

    public decimal MasteryLevel { get; set; } 
}