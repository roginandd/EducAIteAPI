using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDocumentIdWithNoteIdInFlashcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "Flashcards",
                newName: "NoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_DocumentId",
                table: "Flashcards",
                newName: "IX_Flashcards_NoteId");

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveWrongCount",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Notes_NoteId",
                table: "Flashcards",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Notes_NoteId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "ConsecutiveWrongCount",
                table: "StudentFlashcards");

            migrationBuilder.RenameColumn(
                name: "NoteId",
                table: "Flashcards",
                newName: "DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_NoteId",
                table: "Flashcards",
                newName: "IX_Flashcards_DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
