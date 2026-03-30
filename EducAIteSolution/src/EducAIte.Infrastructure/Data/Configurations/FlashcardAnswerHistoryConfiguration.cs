namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlashcardAnswerHistoryConfiguration : IEntityTypeConfiguration<FlashcardAnswerHistory>
{
    public void Configure(EntityTypeBuilder<FlashcardAnswerHistory> builder)
    {
        builder.ToTable("FlashcardAnswerHistories");

        builder.HasKey(history => history.FlashcardAnswerHistoryId);

        builder.Property(history => history.FlashcardAnswerHistoryId).ValueGeneratedOnAdd();
        builder.Property(history => history.SubmittedAnswer).IsRequired().HasMaxLength(2000);
        builder.Property(history => history.ExpectedAnswerSnapshot).IsRequired().HasMaxLength(2000);

        builder.Property(history => history.ScoringSource)
            .HasConversion<int>()
            .HasDefaultValue(AnswerScoringSource.FallbackRules);

        builder.Property(history => history.IsDeleted).HasDefaultValue(false);

        builder.Property(history => history.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(history => history.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasQueryFilter(history => !history.IsDeleted);

        builder.HasOne(history => history.FlashcardSession)
            .WithMany()
            .HasForeignKey(history => history.FlashcardSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(history => history.FlashcardSessionItem)
            .WithMany()
            .HasForeignKey(history => history.FlashcardSessionItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(history => history.StudentFlashcard)
            .WithMany()
            .HasForeignKey(history => history.StudentFlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(history => history.Evaluation)
            .WithOne(evaluation => evaluation.FlashcardAnswerHistory)
            .HasForeignKey<FlashcardAnswerEvaluation>(evaluation => evaluation.FlashcardAnswerHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(history => new { history.StudentFlashcardId, history.AnsweredAt });
    }
}
