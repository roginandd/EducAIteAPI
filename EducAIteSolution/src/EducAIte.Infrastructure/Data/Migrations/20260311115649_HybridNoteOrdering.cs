using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HybridNoteOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_DocumentId",
                table: "Notes");

            migrationBuilder.AlterColumn<decimal>(
                name: "SequenceNumber",
                table: "Notes",
                type: "numeric(30,15)",
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes",
                columns: new[] { "DocumentId", "SequenceNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_DocumentId_SequenceNumber",
                table: "Notes");

            migrationBuilder.AlterColumn<long>(
                name: "SequenceNumber",
                table: "Notes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(decimal),
                oldType: "numeric(30,15)",
                oldDefaultValue: 100m);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DocumentId",
                table: "Notes",
                column: "DocumentId");
        }
    }
}
