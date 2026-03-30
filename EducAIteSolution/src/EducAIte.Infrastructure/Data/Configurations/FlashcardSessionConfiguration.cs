namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class FlashcardSessionConfiguration : IEntityTypeConfiguration<FlashcardSession>
{
    public void Configure(EntityTypeBuilder<FlashcardSession> builder)
    {
        builder.ToTable("FlashcardSessions");

        builder.HasKey(session => session.FlashcardSessionId);

        builder.Property(session => session.FlashcardSessionId).ValueGeneratedOnAdd();

        builder.Property(session => session.ScopeType)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardSessionScopeType.Course);

        builder.Property(session => session.Status)
            .HasConversion<int>()
            .HasDefaultValue(FlashcardSessionStatus.InProgress);

        builder.Property(session => session.IsDeleted).HasDefaultValue(false);

        builder.Property(session => session.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(session => session.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasQueryFilter(session => !session.IsDeleted);

        builder.HasOne(session => session.Student)
            .WithMany(student => student.FlashcardSessions)
            .HasForeignKey(session => session.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(session => session.StudentCourse)
            .WithMany(studentCourse => studentCourse.FlashcardSessions)
            .HasForeignKey(session => session.StudentCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(session => new { session.StudentId, session.StudentCourseId, session.ScopeType, session.Status });
    }
}
