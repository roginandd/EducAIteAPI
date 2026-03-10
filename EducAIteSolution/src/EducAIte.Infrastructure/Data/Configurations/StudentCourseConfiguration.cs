namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        builder.ToTable("StudentCourses");

        builder.HasKey(sc => sc.StudentCourseId);

        builder.Property(sc => sc.StudentCourseId)
            .ValueGeneratedOnAdd();

        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");
        
        builder.Property(sc => sc.UpdatedAt)            
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(sc => sc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(sc => sc.Course)
            .WithMany() 
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.StudyLoad)
            .WithMany() 
            .HasForeignKey(sc => sc.StudyLoadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sc => new { sc.CourseId, sc.StudyLoadId })
            .HasDatabaseName("UX_StudentCourses_Course_StudyLoad")
            .IsUnique();

        builder.HasIndex(sc => sc.CourseId)
            .HasDatabaseName("IX_StudentCourses_CourseId");

        builder.HasIndex(sc => sc.StudyLoadId)
            .HasDatabaseName("IX_StudentCourses_StudyLoadId");
    }
}