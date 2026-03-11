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

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(f => f.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(f => f.Course)
            .WithMany()
            .HasForeignKey(f => f.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Note)
            .WithMany()
            .HasForeignKey(f => f.NoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.Document)
            .WithMany()
            .HasForeignKey(f => f.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
