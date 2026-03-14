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
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(f => !f.IsDeleted);

        // Foreign keys
        builder.HasOne(f => f.Document)
            .WithMany(d => d.Flashcards)
            .HasForeignKey(f => f.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
