using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentFlashcardRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveCorrectCount",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LapseCount",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastReviewOutcome",
                table: "StudentFlashcards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewedAt",
                table: "StudentFlashcards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewAt",
                table: "StudentFlashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFlashcards_StudentId_NextReviewAt",
                table: "StudentFlashcards",
                columns: new[] { "StudentId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFlashcards_StudentId_State_NextReviewAt",
                table: "StudentFlashcards",
                columns: new[] { "StudentId", "State", "NextReviewAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentFlashcards_StudentId_NextReviewAt",
                table: "StudentFlashcards");

            migrationBuilder.DropIndex(
                name: "IX_StudentFlashcards_StudentId_State_NextReviewAt",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "ConsecutiveCorrectCount",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "LapseCount",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "LastReviewOutcome",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "LastReviewedAt",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "NextReviewAt",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "State",
                table: "StudentFlashcards");
        }
    }
}
