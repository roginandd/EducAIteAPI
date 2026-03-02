using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqueConstraintsWithCertifcationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_Students_StudentId",
                table: "StudentCourses");

            migrationBuilder.DropTable(
                name: "StudyLoadCourses");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourses_StudentId_CourseId",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "MasteryLevel",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "SchoolYearEnd",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "SchoolYearStart",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "StudentCourses");

            migrationBuilder.RenameIndex(
                name: "IX_Students_StudentIdNumber",
                table: "Students",
                newName: "UX_Students_StudentIdNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Students_Email",
                table: "Students",
                newName: "UX_Students_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_ExternalId",
                table: "Notes",
                newName: "UX_Notes_ExternalId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_StudentCourseId_GradeType",
                table: "Grades",
                newName: "IX_Grades_StudentCourse_GradeType");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_StudentId_FolderKey",
                table: "Folders",
                newName: "UX_Folders_Student_FolderKey");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_ExternalId",
                table: "Folders",
                newName: "UX_Folders_ExternalId");

            migrationBuilder.RenameIndex(
                name: "IX_Flashcards_ExternalId",
                table: "Flashcards",
                newName: "UX_Flashcards_ExternalId");

            migrationBuilder.RenameIndex(
                name: "IX_FileMetadata_StorageKey",
                table: "FileMetadata",
                newName: "UX_FileMetadata_StorageKey");

            migrationBuilder.RenameIndex(
                name: "IDX_Documents_ExternalId",
                table: "Documents",
                newName: "UX_Documents_ExternalId");

            migrationBuilder.RenameIndex(
                name: "IDX_Courses_EDPCode",
                table: "Courses",
                newName: "UX_Courses_EDPCode");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudyLoads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StudyLoads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudyLoads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Students",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentFlashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentFlashcards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentFlashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<long>(
                name: "StudentId",
                table: "StudentCourses",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentCourses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentCourses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentCourses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                table: "Notes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Grades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Grades",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Grades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Folders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Folders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Flashcards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "NoteId1",
                table: "Flashcards",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "FileMetadata",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FileMetadata",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FileMetadata",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timezone('utc', now())",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "StudyLoadId",
                table: "Courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    CertificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    CertificationKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.CertificationId);
                    table.ForeignKey(
                        name: "FK_Certifications_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    ResumeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ResumeKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.ResumeId);
                    table.ForeignKey(
                        name: "FK_Resumes_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudentId",
                table: "StudentCourses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudyLoadId",
                table: "StudentCourses",
                column: "StudyLoadId");

            migrationBuilder.CreateIndex(
                name: "UX_StudentCourses_Course_StudyLoad",
                table: "StudentCourses",
                columns: new[] { "CourseId", "StudyLoadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_NoteId1",
                table: "Flashcards",
                column: "NoteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StudyLoadId",
                table: "Courses",
                column: "StudyLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_StudentId",
                table: "Certifications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UX_Certifications_CertificationKey",
                table: "Certifications",
                column: "CertificationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_StudentId",
                table: "Resumes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UX_Resumes_ResumeKey",
                table: "Resumes",
                column: "ResumeKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_StudyLoads_StudyLoadId",
                table: "Courses",
                column: "StudyLoadId",
                principalTable: "StudyLoads",
                principalColumn: "StudyLoadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Notes_NoteId1",
                table: "Flashcards",
                column: "NoteId1",
                principalTable: "Notes",
                principalColumn: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_Students_StudentId",
                table: "StudentCourses",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_StudyLoads_StudyLoadId",
                table: "StudentCourses",
                column: "StudyLoadId",
                principalTable: "StudyLoads",
                principalColumn: "StudyLoadId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_StudyLoads_StudyLoadId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Notes_NoteId1",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_Students_StudentId",
                table: "StudentCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_StudyLoads_StudyLoadId",
                table: "StudentCourses");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourses_StudentId",
                table: "StudentCourses");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourses_StudyLoadId",
                table: "StudentCourses");

            migrationBuilder.DropIndex(
                name: "UX_StudentCourses_Course_StudyLoad",
                table: "StudentCourses");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_NoteId1",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "IX_Courses_StudyLoadId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudyLoads");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentFlashcards");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentCourses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "NoteId1",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FileMetadata");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FileMetadata");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StudyLoadId",
                table: "Courses");

            migrationBuilder.RenameIndex(
                name: "UX_Students_StudentIdNumber",
                table: "Students",
                newName: "IX_Students_StudentIdNumber");

            migrationBuilder.RenameIndex(
                name: "UX_Students_Email",
                table: "Students",
                newName: "IX_Students_Email");

            migrationBuilder.RenameIndex(
                name: "UX_Notes_ExternalId",
                table: "Notes",
                newName: "IX_Notes_ExternalId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_StudentCourse_GradeType",
                table: "Grades",
                newName: "IX_Grades_StudentCourseId_GradeType");

            migrationBuilder.RenameIndex(
                name: "UX_Folders_Student_FolderKey",
                table: "Folders",
                newName: "IX_Folders_StudentId_FolderKey");

            migrationBuilder.RenameIndex(
                name: "UX_Folders_ExternalId",
                table: "Folders",
                newName: "IX_Folders_ExternalId");

            migrationBuilder.RenameIndex(
                name: "UX_Flashcards_ExternalId",
                table: "Flashcards",
                newName: "IX_Flashcards_ExternalId");

            migrationBuilder.RenameIndex(
                name: "UX_FileMetadata_StorageKey",
                table: "FileMetadata",
                newName: "IX_FileMetadata_StorageKey");

            migrationBuilder.RenameIndex(
                name: "UX_Documents_ExternalId",
                table: "Documents",
                newName: "IDX_Documents_ExternalId");

            migrationBuilder.RenameIndex(
                name: "UX_Courses_EDPCode",
                table: "Courses",
                newName: "IDX_Courses_EDPCode");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudyLoads",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StudyLoads",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Students",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<decimal>(
                name: "MasteryLevel",
                table: "StudentFlashcards",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0.0m);

            migrationBuilder.AlterColumn<long>(
                name: "StudentId",
                table: "StudentCourses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchoolYearEnd",
                table: "StudentCourses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchoolYearStart",
                table: "StudentCourses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "StudentCourses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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
                table: "Folders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "FileMetadata",
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
                name: "UpdatedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timezone('utc', now())");

            migrationBuilder.CreateTable(
                name: "StudyLoadCourses",
                columns: table => new
                {
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    StudyLoadId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyLoadCourses", x => new { x.CourseId, x.StudyLoadId });
                    table.ForeignKey(
                        name: "FK_StudyLoadCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyLoadCourses_StudyLoads_StudyLoadId",
                        column: x => x.StudyLoadId,
                        principalTable: "StudyLoads",
                        principalColumn: "StudyLoadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudentId_CourseId",
                table: "StudentCourses",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyLoadCourses_StudyLoadId_CourseId",
                table: "StudyLoadCourses",
                columns: new[] { "StudyLoadId", "CourseId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_Students_StudentId",
                table: "StudentCourses",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
