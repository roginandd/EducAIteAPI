using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNodeOwnershipInFlashcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Courses_CourseId",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Notes_NoteId",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Notes_NoteId1",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFlashcards_Folders_FolderId",
                table: "StudentFlashcards");

            migrationBuilder.DropIndex(
                name: "IX_StudentFlashcards_FolderId",
                table: "StudentFlashcards");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_CourseId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_NoteId",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_NoteId1",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "NoteId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "NoteId1",
                table: "Flashcards");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<long>(
                name: "DocumentId",
                table: "Flashcards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards");

            migrationBuilder.AddColumn<long>(
                name: "FolderId",
                table: "StudentFlashcards",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "DocumentId",
                table: "Flashcards",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "CourseId",
                table: "Flashcards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NoteId",
                table: "Flashcards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NoteId1",
                table: "Flashcards",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFlashcards_FolderId",
                table: "StudentFlashcards",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CourseId",
                table: "Flashcards",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_NoteId",
                table: "Flashcards",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_NoteId1",
                table: "Flashcards",
                column: "NoteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Courses_CourseId",
                table: "Flashcards",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Notes_NoteId",
                table: "Flashcards",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "NoteId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Notes_NoteId1",
                table: "Flashcards",
                column: "NoteId1",
                principalTable: "Notes",
                principalColumn: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFlashcards_Folders_FolderId",
                table: "StudentFlashcards",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "FolderId");
        }
    }
}
