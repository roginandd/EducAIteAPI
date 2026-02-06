using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDocumentFolderRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_FolderId1",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FolderId1",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FolderId1",
                table: "Documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FolderId1",
                table: "Documents",
                column: "FolderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId1",
                table: "Documents",
                column: "FolderId1",
                principalTable: "Folders",
                principalColumn: "FolderId");
        }
    }
}
