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

        builder.Property(sc => sc.Semester)
            .IsRequired();

        // Foreign keys
        builder.HasOne(sc => sc.Course)
            .WithMany()
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.Student)
            .WithMany(s => s.EnrolledCourses)
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SchoolYear as owned entity
        builder.OwnsOne(sc => sc.SchoolYear, sy =>
        {
            sy.Property(s => s.StartYear).HasColumnName("SchoolYearStart");
            sy.Property(s => s.EndYear).HasColumnName("SchoolYearEnd");
        });

        // Composite index for performance
        builder.HasIndex(sc => new { sc.StudentId, sc.CourseId })
            .IsUnique();
    }
}