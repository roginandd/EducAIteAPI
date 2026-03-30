namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlashcardAnswerEvaluationConfiguration : IEntityTypeConfiguration<FlashcardAnswerEvaluation>
{
    public void Configure(EntityTypeBuilder<FlashcardAnswerEvaluation> builder)
    {
        builder.ToTable("FlashcardAnswerEvaluations");

        builder.HasKey(evaluation => evaluation.FlashcardAnswerEvaluationId);

        builder.Property(evaluation => evaluation.FlashcardAnswerEvaluationId)
            .ValueGeneratedOnAdd();

        builder.Property(evaluation => evaluation.Verdict)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(evaluation => evaluation.AcceptedAsCorrect)
            .IsRequired();

        builder.Property(evaluation => evaluation.QualityScore)
            .IsRequired();

        builder.Property(evaluation => evaluation.FeedbackSummary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(evaluation => evaluation.SemanticRationale)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue(string.Empty);

        builder.Property(evaluation => evaluation.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(evaluation => evaluation.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");
    }
}
