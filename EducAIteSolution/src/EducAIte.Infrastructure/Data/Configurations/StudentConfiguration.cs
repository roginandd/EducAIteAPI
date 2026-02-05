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
            .HasMaxLength(100);

        builder.Property(s => s.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.HasIndex(s => s.StudentIdNumber)
            .IsUnique();

        builder.HasIndex(s => s.Email)
            .IsUnique();

        // Navigation properties - configured from the other entity side
        // Folders relationship is configured in FolderConfiguration
        // UploadedFiles relationship is configured in FileMetadataConfiguration
        // Flashcards relationship is configured in StudentFlashcardConfiguration
        // EnrolledCourses relationship is configured in StudentCourseConfiguration
    }
}