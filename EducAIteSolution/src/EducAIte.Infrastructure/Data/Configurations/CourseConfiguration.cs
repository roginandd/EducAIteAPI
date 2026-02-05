namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.CourseId);

        builder.Property(c => c.CourseId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.EDPCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.CourseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Units)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.HasIndex(c => c.EDPCode)
            .HasDatabaseName("IDX_Courses_EDPCode")
            .IsUnique();
    }
}


