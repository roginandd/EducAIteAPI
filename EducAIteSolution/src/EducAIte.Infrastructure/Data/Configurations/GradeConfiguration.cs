namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");

        builder.HasKey(g => g.GradeId);

        builder.Property(g => g.GradeId)
            .ValueGeneratedOnAdd();

        builder.Property(g => g.GradeValue)
            .HasPrecision(3, 2)
            .IsRequired();

        builder.Property(g => g.GradeType)
            .IsRequired();

        // Composite index for performance
        builder.HasIndex(g => new { g.StudentCourseId, g.GradeType });
    }
}