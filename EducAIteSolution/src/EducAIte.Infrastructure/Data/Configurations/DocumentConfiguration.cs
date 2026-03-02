namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.DocumentId);

        builder.Property(d => d.DocumentId)
            .ValueGeneratedOnAdd();

        builder.Property(d => d.ExternalId)
            .IsRequired();

        builder.Property(d => d.DocumentName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.FileMetadata)
            .WithMany()
            .HasForeignKey(d => d.FileMetadataId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ExternalId)
            .HasDatabaseName("UX_Documents_ExternalId")
            .IsUnique();
    }
}