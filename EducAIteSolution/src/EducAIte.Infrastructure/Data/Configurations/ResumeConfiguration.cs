namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");

        builder.HasKey(r => r.ResumeId);

        builder.Property(r => r.ResumeId)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.ResumeKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(r => r.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        // Relationship: Student has many Resumes
        builder.HasOne(r => r.Student)
            .WithMany() // Student may not need a collection of all resumes
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ResumeKey)
            .HasDatabaseName("UX_Resumes_ResumeKey")
            .IsUnique();

        builder.HasIndex(r => r.StudentId)
            .HasDatabaseName("IX_Resumes_StudentId");
    }
}