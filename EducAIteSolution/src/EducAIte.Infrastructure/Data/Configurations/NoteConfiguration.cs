namespace EducAIte.Infrastructure.Data.Configurations;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(n => n.NoteId);

        builder.Property(n => n.NoteId)
            .ValueGeneratedOnAdd();

        builder.Property(n => n.ExternalId)
            .IsRequired();

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.NoteContent)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        // Foreign key
        builder.HasOne(n => n.Document)
            .WithMany()
            .HasForeignKey(n => n.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.ExternalId)
            .IsUnique();
    }
}