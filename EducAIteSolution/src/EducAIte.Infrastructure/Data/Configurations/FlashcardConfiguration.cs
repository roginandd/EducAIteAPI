namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("Flashcards");

        builder.HasKey(f => f.FlashcardId);

        builder.Property(f => f.FlashcardId)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Question)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.Answer)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.ConceptExplanation)
            .IsRequired()
            .HasMaxLength(4000)
            .HasDefaultValue(string.Empty);

        builder.Property(f => f.AnsweringGuidance)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue(string.Empty);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(f => !f.IsDeleted);

        builder.HasOne(f => f.Note)
            .WithMany(n => n.Flashcards)
            .HasForeignKey(f => f.NoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(f => f.StudentFlashcards)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(f => f.AcceptedAnswerAliases)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
