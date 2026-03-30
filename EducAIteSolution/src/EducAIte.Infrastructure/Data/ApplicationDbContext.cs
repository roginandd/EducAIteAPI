namespace EducAIte.Infrastructure.Data;

using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<FileMetadata> FileMetadata { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardAcceptedAnswerAlias> FlashcardAcceptedAnswerAliases { get; set; }
    public DbSet<FlashcardAnswerEvaluation> FlashcardAnswerEvaluations { get; set; }
    public DbSet<FlashcardAnswerHistory> FlashcardAnswerHistories { get; set; }
    public DbSet<FlashcardSession> FlashcardSessions { get; set; }
    public DbSet<FlashcardSessionItem> FlashcardSessionItems { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentCourse> StudentCourses { get; set; }
    public DbSet<StudentCoursePerformanceSummary> StudentCoursePerformanceSummaries { get; set; }
    public DbSet<StudentFlashcard> StudentFlashcards { get; set; }
    public DbSet<StudentFlashcardAnalytics> StudentFlashcardAnalytics { get; set; }
    public DbSet<StudentOverallPerformanceSummary> StudentOverallPerformanceSummaries { get; set; }
    public DbSet<StudyLoad> StudyLoads { get; set; }



    // Configure entity mappings using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // reads all the configuration classes in the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    

}
