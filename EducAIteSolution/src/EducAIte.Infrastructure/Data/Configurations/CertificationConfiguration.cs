namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("Certifications");

        builder.HasKey(c => c.CertificationId);

        builder.Property(c => c.CertificationId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.CertificationKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        // Relationship: Student has many Certifications
        builder.HasOne(c => c.Student)
            .WithMany(s => s.Certifications)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.CertificationKey)
            .HasDatabaseName("UX_Certifications_CertificationKey")
            .IsUnique();

        builder.HasIndex(c => c.StudentId)
            .HasDatabaseName("IX_Certifications_StudentId");
    }
}