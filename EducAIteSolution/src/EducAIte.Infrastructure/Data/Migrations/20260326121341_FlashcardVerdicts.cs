using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlashcardVerdicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastEvaluationVerdict",
                table: "StudentFlashcards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastQualityScore",
                table: "StudentFlashcards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnsweringGuidance",
                table: "Flashcards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConceptExplanation",
                table: "Flashcards",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FlashcardAcceptedAnswerAliases",
                columns: table => new
                {
                    FlashcardAcceptedAnswerAliasId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    Alias = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardAcceptedAnswerAliases", x => x.FlashcardAcceptedAnswerAliasId);
                    table.ForeignKey(
                        name: "FK_FlashcardAcceptedAnswerAliases_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "FlashcardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardAnswerEvaluations",
                columns: table => new
                {
                    FlashcardAnswerEvaluationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlashcardAnswerHistoryId = table.Column<long>(type: "bigint", nullable: false),
                    Verdict = table.Column<int>(type: "integer", nullable: false),
                    AcceptedAsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    FeedbackSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SemanticRationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: ""),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardAnswerEvaluations", x => x.FlashcardAnswerEvaluationId);
                    table.ForeignKey(
                        name: "FK_FlashcardAnswerEvaluations_FlashcardAnswerHistories_Flashca~",
                        column: x => x.FlashcardAnswerHistoryId,
                        principalTable: "FlashcardAnswerHistories",
                        principalColumn: "FlashcardAnswerHistoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAcceptedAnswerAliases_FlashcardId_Order",
                table: "FlashcardAcceptedAnswerAliases",
                columns: new[] { "FlashcardId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAnswerEvaluations_FlashcardAnswerHistoryId",
                table: "FlashcardAnswerEvaluations",
                column: "FlashcardAnswerHistoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardAcceptedAnswerAliases");

            migrationBuilder.DropTable(
                name: "FlashcardAnswerEvaluations");

            migrationBuilder.DropColumn(
                name: "LastEvaluationVerdict",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "LastQualityScore",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "AnsweringGuidance",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "ConceptExplanation",
                table: "Flashcards");
        }
    }
}
