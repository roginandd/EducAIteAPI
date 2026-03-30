using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AiANALYTICSENTITIES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Courses_CourseId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_StudentFlashcards_StudentId_State_NextReviewAt",
                table: "StudentFlashcards");

            migrationBuilder.DropIndex(
                name: "IX_Folders_CourseId",
                table: "Folders");

            migrationBuilder.AddColumn<long>(
                name: "StudentCourseId",
                table: "Folders",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Folders" AS f
                SET "StudentCourseId" = sc."StudentCourseId"
                FROM "StudentCourses" AS sc
                INNER JOIN "StudyLoads" AS sl
                    ON sc."StudyLoadId" = sl."StudyLoadId"
                WHERE f."CourseId" IS NOT NULL
                  AND f."CourseId" = sc."CourseId"
                  AND f."StudentId" = sl."StudentId"
                  AND f."SchoolYearStart" = sl."SchoolYearStart"
                  AND f."SchoolYearEnd" = sl."SchoolYearEnd"
                  AND f."Semester" = CAST(sl."Semester" AS smallint);
                """);

            migrationBuilder.Sql(
                """
                WITH RECURSIVE folder_course AS (
                    SELECT f."FolderId", f."ParentFolderId", f."StudentCourseId"
                    FROM "Folders" AS f
                    WHERE f."StudentCourseId" IS NOT NULL

                    UNION ALL

                    SELECT child."FolderId", child."ParentFolderId", parent."StudentCourseId"
                    FROM "Folders" AS child
                    INNER JOIN folder_course AS parent
                        ON child."ParentFolderId" = parent."FolderId"
                    WHERE child."StudentCourseId" IS NULL
                )
                UPDATE "Folders" AS target
                SET "StudentCourseId" = source."StudentCourseId"
                FROM folder_course AS source
                WHERE target."FolderId" = source."FolderId"
                  AND target."StudentCourseId" IS NULL
                  AND source."StudentCourseId" IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Folders" WHERE "StudentCourseId" IS NULL) THEN
                        RAISE EXCEPTION 'Folders migration aborted: one or more folders could not be mapped to a StudentCourse.';
                    END IF;
                END
                $$;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "StudentCourseId",
                table: "Folders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "State",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Folders");

            migrationBuilder.CreateTable(
                name: "FlashcardSessions",
                columns: table => new
                {
                    FlashcardSessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StudentCourseId = table.Column<long>(type: "bigint", nullable: true),
                    ScopeType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CurrentItemIndex = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RestartedFromSessionId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardSessions", x => x.FlashcardSessionId);
                    table.ForeignKey(
                        name: "FK_FlashcardSessions_StudentCourses_StudentCourseId",
                        column: x => x.StudentCourseId,
                        principalTable: "StudentCourses",
                        principalColumn: "StudentCourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashcardSessions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCoursePerformanceSummaries",
                columns: table => new
                {
                    StudentCourseId = table.Column<long>(type: "bigint", nullable: false),
                    TrackedFlashcardsCount = table.Column<int>(type: "integer", nullable: false),
                    MasteredFlashcardsCount = table.Column<int>(type: "integer", nullable: false),
                    FlashcardAccuracyRate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    LearningRetentionRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OverallPerformanceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    AiInsight = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImprovementSuggestion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AiStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    AiLastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCoursePerformanceSummaries", x => x.StudentCourseId);
                    table.ForeignKey(
                        name: "FK_StudentCoursePerformanceSummaries_StudentCourses_StudentCou~",
                        column: x => x.StudentCourseId,
                        principalTable: "StudentCourses",
                        principalColumn: "StudentCourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFlashcardAnalytics",
                columns: table => new
                {
                    StudentFlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    LastAnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EaseFactor = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 2.50m),
                    RepetitionCount = table.Column<int>(type: "integer", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    LapseCount = table.Column<int>(type: "integer", nullable: false),
                    MasteryLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    ConsistencyScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    RetentionScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    AiInsight = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImprovementSuggestion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AiStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    AiLastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFlashcardAnalytics", x => x.StudentFlashcardId);
                    table.ForeignKey(
                        name: "FK_StudentFlashcardAnalytics_StudentFlashcards_StudentFlashcar~",
                        column: x => x.StudentFlashcardId,
                        principalTable: "StudentFlashcards",
                        principalColumn: "StudentFlashcardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentOverallPerformanceSummaries",
                columns: table => new
                {
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    TrackedCoursesCount = table.Column<int>(type: "integer", nullable: false),
                    TrackedFlashcardsCount = table.Column<int>(type: "integer", nullable: false),
                    MasteredFlashcardsCount = table.Column<int>(type: "integer", nullable: false),
                    FlashcardAccuracyRate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    LearningRetentionRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OverallPerformanceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    AiInsight = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImprovementSuggestion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AiStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    AiLastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentOverallPerformanceSummaries", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_StudentOverallPerformanceSummaries_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardSessionItems",
                columns: table => new
                {
                    FlashcardSessionItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlashcardSessionId = table.Column<long>(type: "bigint", nullable: false),
                    StudentFlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalOrder = table.Column<int>(type: "integer", nullable: false),
                    CurrentOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PresentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAnswerHistoryId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardSessionItems", x => x.FlashcardSessionItemId);
                    table.ForeignKey(
                        name: "FK_FlashcardSessionItems_FlashcardSessions_FlashcardSessionId",
                        column: x => x.FlashcardSessionId,
                        principalTable: "FlashcardSessions",
                        principalColumn: "FlashcardSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardSessionItems_StudentFlashcards_StudentFlashcardId",
                        column: x => x.StudentFlashcardId,
                        principalTable: "StudentFlashcards",
                        principalColumn: "StudentFlashcardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardAnswerHistories",
                columns: table => new
                {
                    FlashcardAnswerHistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlashcardSessionId = table.Column<long>(type: "bigint", nullable: true),
                    FlashcardSessionItemId = table.Column<long>(type: "bigint", nullable: true),
                    StudentFlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedAnswer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpectedAnswerSnapshot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: false),
                    AiQualityScore = table.Column<int>(type: "integer", nullable: true),
                    FallbackQualityScore = table.Column<int>(type: "integer", nullable: true),
                    FinalQualityScore = table.Column<int>(type: "integer", nullable: false),
                    WasAcceptedAsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScoringSource = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardAnswerHistories", x => x.FlashcardAnswerHistoryId);
                    table.ForeignKey(
                        name: "FK_FlashcardAnswerHistories_FlashcardSessionItems_FlashcardSes~",
                        column: x => x.FlashcardSessionItemId,
                        principalTable: "FlashcardSessionItems",
                        principalColumn: "FlashcardSessionItemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FlashcardAnswerHistories_FlashcardSessions_FlashcardSession~",
                        column: x => x.FlashcardSessionId,
                        principalTable: "FlashcardSessions",
                        principalColumn: "FlashcardSessionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FlashcardAnswerHistories_StudentFlashcards_StudentFlashcard~",
                        column: x => x.StudentFlashcardId,
                        principalTable: "StudentFlashcards",
                        principalColumn: "StudentFlashcardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_StudentCourseId",
                table: "Folders",
                column: "StudentCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAnswerHistories_FlashcardSessionId",
                table: "FlashcardAnswerHistories",
                column: "FlashcardSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAnswerHistories_FlashcardSessionItemId",
                table: "FlashcardAnswerHistories",
                column: "FlashcardSessionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardAnswerHistories_StudentFlashcardId_AnsweredAt",
                table: "FlashcardAnswerHistories",
                columns: new[] { "StudentFlashcardId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSessionItems_FlashcardSessionId_CurrentOrder",
                table: "FlashcardSessionItems",
                columns: new[] { "FlashcardSessionId", "CurrentOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSessionItems_StudentFlashcardId",
                table: "FlashcardSessionItems",
                column: "StudentFlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSessions_StudentCourseId",
                table: "FlashcardSessions",
                column: "StudentCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSessions_StudentId_StudentCourseId_ScopeType_Status",
                table: "FlashcardSessions",
                columns: new[] { "StudentId", "StudentCourseId", "ScopeType", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_StudentCourses_StudentCourseId",
                table: "Folders",
                column: "StudentCourseId",
                principalTable: "StudentCourses",
                principalColumn: "StudentCourseId",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_StudentCourses_StudentCourseId",
                table: "Folders");

            migrationBuilder.DropTable(
                name: "FlashcardAnswerHistories");

            migrationBuilder.DropTable(
                name: "StudentCoursePerformanceSummaries");

            migrationBuilder.DropTable(
                name: "StudentFlashcardAnalytics");

            migrationBuilder.DropTable(
                name: "StudentOverallPerformanceSummaries");

            migrationBuilder.DropTable(
                name: "FlashcardSessionItems");

            migrationBuilder.DropTable(
                name: "FlashcardSessions");

            migrationBuilder.DropIndex(
                name: "IX_Folders_StudentCourseId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "StudentCourseId",
                table: "Folders");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "StudentFlashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "CourseId",
                table: "Folders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFlashcards_StudentId_State_NextReviewAt",
                table: "StudentFlashcards",
                columns: new[] { "StudentId", "State", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CourseId",
                table: "Folders",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Courses_CourseId",
                table: "Folders",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
