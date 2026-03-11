using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DocumentAndNoteWithoutGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Documents_DocumentId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "UX_Notes_ExternalId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "UX_Folders_ExternalId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "UX_Flashcards_ExternalId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "UX_Documents_ExternalId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Documents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "FolderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Documents_DocumentId",
                table: "Notes",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Documents_DocumentId",
                table: "Notes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Folders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Flashcards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "UX_Notes_ExternalId",
                table: "Notes",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Folders_ExternalId",
                table: "Folders",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Flashcards_ExternalId",
                table: "Flashcards",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Documents_ExternalId",
                table: "Documents",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "FolderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Documents_DocumentId",
                table: "Notes",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
