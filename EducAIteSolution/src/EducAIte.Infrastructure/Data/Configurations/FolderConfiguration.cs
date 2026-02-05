namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("Folders");

        builder.HasKey(f => f.FolderId);

        builder.Property(f => f.FolderId)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.ExternalId)
            .IsRequired();

        builder.Property(f => f.Semester)
            .IsRequired();

        builder.Property(f => f.FolderKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(f => f.UpdatedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne(f => f.Student)
            .WithMany(s => s.Folders)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Course)
            .WithMany()
            .HasForeignKey(f => f.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Self-referencing relationship for parent/child folders
        builder.HasOne(f => f.ParentFolder)
            .WithMany(f => f.SubFolders)
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.ExternalId)
            .IsUnique();

        builder.HasIndex(f => new { f.StudentId, f.FolderKey })
            .IsUnique();
    }
}