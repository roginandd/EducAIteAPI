namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.StudentId);

        builder.Property(s => s.StudentId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.StudentIdNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Program)
            .IsRequired();

        builder.Property(s => s.BirthDate)
            .IsRequired();

        builder.Property(s => s.Semester)
            .IsRequired();

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.MiddleName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(s => s.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(s => s.StudentIdNumber)
            .HasDatabaseName("UX_Students_StudentIdNumber")
            .IsUnique();

        builder.HasIndex(s => s.Email)
            .HasDatabaseName("UX_Students_Email")
            .IsUnique();

        // Navigation properties - configured from the other entity side
        // Folders relationship is configured in FolderConfiguration
        // UploadedFiles relationship is configured in FileMetadataConfiguration
        // Flashcards relationship is configured in StudentFlashcardConfiguration
        // Certifications relationship is configured in CertificationConfiguration
    }
}
