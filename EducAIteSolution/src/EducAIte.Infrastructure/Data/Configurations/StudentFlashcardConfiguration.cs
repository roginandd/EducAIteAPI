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

        builder.Property(sf => sf.ConsecutiveCorrectCount)
            .HasDefaultValue(0);

        builder.Property(sf => sf.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(sf => sf.LapseCount)
            .HasDefaultValue(0);

        builder.Property(sf => sf.LastQualityScore);

        builder.Property(sf => sf.NextReviewAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(sf => sf.LastReviewedAt);

        builder.Property(sf => sf.LastReviewOutcome)
            .HasConversion<int?>();

        builder.Property(sf => sf.LastEvaluationVerdict)
            .HasConversion<int?>();

        builder.Property(sf => sf.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");
        
        builder.Property(sf => sf.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(sf => sf.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(sf => !sf.IsDeleted);

        // Foreign keys
        builder.HasOne(sf => sf.Student)
            .WithMany(s => s.StudentFlashcards)
            .HasForeignKey(sf => sf.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sf => sf.Flashcard)
            .WithMany(f => f.StudentFlashcards)
            .HasForeignKey(sf => sf.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index
        builder.HasIndex(sf => new { sf.StudentId, sf.FlashcardId })
            .IsUnique();

        builder.HasIndex(sf => new { sf.StudentId, sf.NextReviewAt });
    }
}
