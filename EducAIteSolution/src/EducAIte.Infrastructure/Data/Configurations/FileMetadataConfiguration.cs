namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("FileMetadata");

        builder.HasKey(fm => fm.FileMetaDataId);

        builder.Property(fm => fm.FileMetaDataId)
            .ValueGeneratedOnAdd();

        builder.Property(fm => fm.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(fm => fm.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(fm => fm.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fm => fm.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(fm => fm.FileSizeInBytes)
            .IsRequired();

        builder.Property(fm => fm.UploadedAt)
            .IsRequired();

        // Foreign key
        builder.HasOne(fm => fm.Student)
            .WithMany(s => s.UploadedFiles)
            .HasForeignKey(fm => fm.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fm => fm.StorageKey)
            .IsUnique();
    }
}