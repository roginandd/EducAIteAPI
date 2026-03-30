namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlashcardAcceptedAnswerAliasConfiguration : IEntityTypeConfiguration<FlashcardAcceptedAnswerAlias>
{
    public void Configure(EntityTypeBuilder<FlashcardAcceptedAnswerAlias> builder)
    {
        builder.ToTable("FlashcardAcceptedAnswerAliases");

        builder.HasKey(alias => alias.FlashcardAcceptedAnswerAliasId);

        builder.Property(alias => alias.FlashcardAcceptedAnswerAliasId)
            .ValueGeneratedOnAdd();

        builder.Property(alias => alias.Alias)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(alias => alias.Order)
            .HasDefaultValue(0);

        builder.Property(alias => alias.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(alias => alias.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasOne(alias => alias.Flashcard)
            .WithMany(flashcard => flashcard.AcceptedAnswerAliases)
            .HasForeignKey(alias => alias.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(alias => new { alias.FlashcardId, alias.Order });
    }
}
