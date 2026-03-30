namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class StudentFlashcardAnalyticsConfiguration : IEntityTypeConfiguration<StudentFlashcardAnalytics>
{
    public void Configure(EntityTypeBuilder<StudentFlashcardAnalytics> builder)
    {
        builder.ToTable("StudentFlashcardAnalytics");

        builder.HasKey(analytics => analytics.StudentFlashcardId);

        builder.Property(analytics => analytics.EaseFactor).HasPrecision(5, 2).HasDefaultValue(2.50m);
        builder.Property(analytics => analytics.ConfidenceScore).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(analytics => analytics.ConsistencyScore).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(analytics => analytics.RetentionScore).HasPrecision(5, 2).HasDefaultValue(0m);

        builder.Property(analytics => analytics.MasteryLevel)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardMasteryLevel.New);

        builder.Property(analytics => analytics.RiskLevel)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardRiskLevel.High);

        builder.Property(analytics => analytics.AiStatus)
            .HasConversion<int>()
            .HasDefaultValue(AiEvaluationStatus.InsufficientSignal);

        builder.Property(analytics => analytics.AiInsight).HasMaxLength(2000);
        builder.Property(analytics => analytics.ImprovementSuggestion).HasMaxLength(2000);

        builder.Property(analytics => analytics.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(analytics => analytics.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasOne(analytics => analytics.StudentFlashcard)
            .WithOne()
            .HasForeignKey<StudentFlashcardAnalytics>(analytics => analytics.StudentFlashcardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
