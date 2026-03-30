namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlashcardSessionItemConfiguration : IEntityTypeConfiguration<FlashcardSessionItem>
{
    public void Configure(EntityTypeBuilder<FlashcardSessionItem> builder)
    {
        builder.ToTable("FlashcardSessionItems");

        builder.HasKey(item => item.FlashcardSessionItemId);

        builder.Property(item => item.FlashcardSessionItemId).ValueGeneratedOnAdd();

        builder.Property(item => item.Status)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardSessionItemStatus.Pending);

        builder.Property(item => item.IsDeleted).HasDefaultValue(false);

        builder.Property(item => item.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(item => item.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasQueryFilter(item => !item.IsDeleted);

        builder.HasOne(item => item.FlashcardSession)
            .WithMany(session => session.Items)
            .HasForeignKey(item => item.FlashcardSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.StudentFlashcard)
            .WithMany()
            .HasForeignKey(item => item.StudentFlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(item => new { item.FlashcardSessionId, item.CurrentOrder });
    }
}
