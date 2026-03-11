using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexSequenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes",
                columns: new[] { "DocumentId", "SequenceNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes",
                columns: new[] { "DocumentId", "SequenceNumber" });
        }
    }
}
