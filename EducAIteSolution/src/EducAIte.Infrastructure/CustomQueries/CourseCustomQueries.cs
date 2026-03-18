using EducAIte.Domain.Entities;
using Npgsql;

namespace EducAIte.Infrastructure.CustomQueries;

/// <summary>
/// Provides custom SQL builders for course-specific batch operations.
/// </summary>
public static class CourseCustomQueries
{
    /// <summary>
    /// Builds a parameterized insert statement that ignores duplicate EDP codes.
    /// </summary>
    /// <param name="courses">The distinct courses to insert.</param>
    /// <returns>The SQL command text and its parameters.</returns>
    public static (string Sql, NpgsqlParameter[] Parameters) BuildInsertMissingCourses(
        IReadOnlyList<Course> courses)
    {
        List<NpgsqlParameter> parameters = new(courses.Count * 3);
        List<string> valueRows = new(courses.Count);

        for (int index = 0; index < courses.Count; index++)
        {
            int b = index * 3;

            // ::int2 matches byte → smallint in PostgreSQL
            valueRows.Add($"(@p{b}::text, @p{b + 1}::text, @p{b + 2}::int2)");

            Course course = courses[index];
            parameters.Add(new NpgsqlParameter($"p{b}", course.EDPCode));
            parameters.Add(new NpgsqlParameter($"p{b + 1}", course.CourseName));
            parameters.Add(new NpgsqlParameter($"p{b + 2}", course.Units));
        }

        string sql = $"""
            INSERT INTO "Courses" ("EDPCode", "CourseName", "Units", "IsDeleted", "CreatedAt", "UpdatedAt")
            SELECT
                v."EDPCode",
                v."CourseName",
                v."Units",
                FALSE,       -- IsDeleted default
                NOW(),       -- CreatedAt
                NOW()        -- UpdatedAt
            FROM (VALUES {string.Join(", ", valueRows)})
                AS v("EDPCode", "CourseName", "Units")
            WHERE NOT EXISTS (
                SELECT 1
                FROM "Courses" c
                WHERE c."EDPCode" = v."EDPCode"
            )
            """;

        return (sql, parameters.ToArray());
    }
}
