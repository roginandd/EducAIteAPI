using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducAIteSolution.src.EducAIte.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueIndexForStudyLoad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StudyLoads_StudentSchoolYearSemesterUnique",
                table: "StudyLoads",
                columns: new[] { "StudentId", "SchoolYearStart", "SchoolYearEnd", "Semester" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyLoads_StudentSchoolYearSemesterUnique",
                table: "StudyLoads");
        }
    }
}
