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

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.NoteContent)
            .IsRequired();

        builder.Property(n => n.SequenceNumber)
            .IsRequired()
            .HasColumnType("numeric(30,15)")
            .HasDefaultValue(100m);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        builder.Property(n => n.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign key: Bidirectional mapping with Document
        builder.HasOne(n => n.Document)
            .WithMany(d => d.Notes)
            .HasForeignKey(n => n.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.DocumentId, n.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("IX_Notes_DocumentId_SequenceNumber");

        builder.HasQueryFilter(note => !note.IsDeleted);

        builder.Navigation(n => n.Flashcards)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
