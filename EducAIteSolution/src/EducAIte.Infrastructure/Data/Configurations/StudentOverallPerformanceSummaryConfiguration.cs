namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class StudentOverallPerformanceSummaryConfiguration : IEntityTypeConfiguration<StudentOverallPerformanceSummary>
{
    public void Configure(EntityTypeBuilder<StudentOverallPerformanceSummary> builder)
    {
        builder.ToTable("StudentOverallPerformanceSummaries");

        builder.HasKey(summary => summary.StudentId);

        builder.Property(summary => summary.FlashcardAccuracyRate).HasPrecision(6, 4);
        builder.Property(summary => summary.LearningRetentionRate).HasPrecision(5, 2);
        builder.Property(summary => summary.ConfidenceScore).HasPrecision(5, 2);
        builder.Property(summary => summary.OverallPerformanceScore).HasPrecision(5, 2);

        builder.Property(summary => summary.RiskLevel)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardRiskLevel.High);

        builder.Property(summary => summary.AiStatus)
            .HasConversion<int>()
            .HasDefaultValue(AiEvaluationStatus.InsufficientSignal);

        builder.Property(summary => summary.AiInsight).HasMaxLength(2000);
        builder.Property(summary => summary.ImprovementSuggestion).HasMaxLength(2000);

        builder.Property(summary => summary.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(summary => summary.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasOne(summary => summary.Student)
            .WithOne()
            .HasForeignKey<StudentOverallPerformanceSummary>(summary => summary.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
