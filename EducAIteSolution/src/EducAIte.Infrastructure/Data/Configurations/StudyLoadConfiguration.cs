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
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(sl => sl.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(sl => sl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(sl => sl.Semester)
            .HasConversion<int>()
            .IsRequired();

        // Configure SchoolYear as owned entity
        builder.OwnsOne(sl => sl.SchoolYear, sy =>
        {
            sy.Property(s => s.StartYear).HasColumnName("SchoolYearStart").IsRequired();
            sy.Property(s => s.EndYear).HasColumnName("SchoolYearEnd").IsRequired();
        });


        // Relationships
        builder.HasOne(sl => sl.Student)
            .WithMany(s => s.StudyLoads)
            .HasForeignKey(sl => sl.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.FileMetadata)
            .WithMany() // Metadata is a generic file reference
            .HasForeignKey(sl => sl.FileMetadataId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sl => sl.StudentId);
        builder.HasIndex(sl => sl.FileMetadataId);
    }
}