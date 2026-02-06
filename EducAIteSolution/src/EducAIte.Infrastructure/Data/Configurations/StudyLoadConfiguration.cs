namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudyLoadConfiguration : IEntityTypeConfiguration<StudyLoad>
{
    public void Configure(EntityTypeBuilder<StudyLoad> builder)
    {
        builder.ToTable("StudyLoads");

        builder.HasKey(sl => sl.StudyLoadId);

        builder.Property(sl => sl.StudyLoadId)
            .ValueGeneratedOnAdd();

        builder.Property(sl => sl.CreatedAt)
            .IsRequired();

        builder.Property(sl => sl.UpdatedAt)
            .IsRequired();

        // Configure SchoolYear as owned entity
        builder.OwnsOne(sl => sl.SchoolYear, sy =>
        {
            sy.Property(s => s.StartYear).HasColumnName("SchoolYearStart");
            sy.Property(s => s.EndYear).HasColumnName("SchoolYearEnd");
        });

        // Foreign keys
        builder.HasOne(sl => sl.Student)
            .WithMany(s => s.StudyLoads)
            .HasForeignKey(sl => sl.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.FileMetadata)
            .WithMany()
            .HasForeignKey(sl => sl.FileMetadataId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many relationship with Course
        builder.HasMany(sl => sl.Courses)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "StudyLoadCourses",
                j => j.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey("CourseId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<StudyLoad>()
                    .WithMany()
                    .HasForeignKey("StudyLoadId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasIndex("StudyLoadId", "CourseId").IsUnique()
            );

        // Ignore computed property
        builder.Ignore(sl => sl.TotalUnits);
    }
}