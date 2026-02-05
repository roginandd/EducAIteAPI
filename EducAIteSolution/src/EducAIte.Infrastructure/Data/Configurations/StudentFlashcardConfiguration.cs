namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentFlashcardConfiguration : IEntityTypeConfiguration<StudentFlashcard>
{
    public void Configure(EntityTypeBuilder<StudentFlashcard> builder)
    {
        builder.ToTable("StudentFlashcards");

        builder.HasKey(sf => sf.StudentFlashcardId);

        builder.Property(sf => sf.StudentFlashcardId)
            .ValueGeneratedOnAdd();

        builder.Property(sf => sf.CorrectCount)
            .HasDefaultValue(0);

        builder.Property(sf => sf.WrongCount)
            .HasDefaultValue(0);

        builder.Property(sf => sf.MasteryLevel)
            .HasPrecision(5, 4)
            .HasDefaultValue(0.0m);

        // Foreign keys
        builder.HasOne(sf => sf.Student)
            .WithMany(s => s.Flashcards)
            .HasForeignKey(sf => sf.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sf => sf.Flashcard)
            .WithMany()
            .HasForeignKey(sf => sf.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index
        builder.HasIndex(sf => new { sf.StudentId, sf.FlashcardId })
            .IsUnique();
    }
}