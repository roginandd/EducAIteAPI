using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Application.Services;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Configuration;

namespace EducAIte.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
      ConfigureMapster();

      // Business Logic and Application Services
      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<ICourseService, CourseService>();
      services.AddScoped<IDocumentService, DocumentService>();
      services.AddScoped<IFlashcardAnswerScoringService, FlashcardAnswerScoringService>();
      services.AddScoped<IFlashcardSessionService, FlashcardSessionService>();
      services.AddScoped<IFlashcardWorkspaceService, FlashcardWorkspaceService>();
      services.AddScoped<IFolderService, FolderService>();
      services.AddScoped<IFlashcardService, FlashcardService>();
      services.AddScoped<INoteService, NoteService>();
      services.AddScoped<INoteOrderingService, NoteOrderingService>();
      services.AddScoped<IPerformanceSummaryService, PerformanceSummaryService>();
      services.AddScoped<IResourceOwnershipService, ResourceOwnershipService>();
      services.AddSingleton<ISqidService, SqidService>();
      services.AddScoped<IStudentCourseService, StudentCourseService>();
      services.AddScoped<IStudentFlashcardService, StudentFlashcardService>();
      services.AddScoped<IStudentFlashcardAnalyticsService, StudentFlashcardAnalyticsService>();
      services.AddScoped<IStudentService, StudentService>();
      services.AddScoped<IStudyLoadService, StudyLoadService>();
      services.AddSingleton<IStudentPerformanceAiWorkQueue, StudentPerformanceAiWorkQueue>();
      services.AddHostedService<StudentPerformanceAiWorker>();
      services.AddHttpClient<IStudentPerformanceAiClient, EducaiteAiStudentPerformanceClient>((serviceProvider, client) =>
      {
          IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
          string baseUrl = configuration["EducaiteAi:BaseUrl"] ?? "http://localhost:3000/";
          client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
      });

      // Repositories
      services.AddScoped<IStudentRepository, StudentRepository>();
      services.AddScoped<ICourseRepository, CourseRepository>();
      services.AddScoped<IStudyLoadRepository, StudyLoadRepository>();
      services.AddScoped<IDocumentRepository, DocumentRepository>();
      services.AddScoped<IFolderRepository, FolderRepository>();
      services.AddScoped<IFlashcardRepository, FlashcardRepository>();
      services.AddScoped<IFlashcardAnswerHistoryRepository, FlashcardAnswerHistoryRepository>();
      services.AddScoped<IFlashcardSessionRepository, FlashcardSessionRepository>();
      services.AddScoped<IFlashcardSessionItemRepository, FlashcardSessionItemRepository>();
      services.AddScoped<INoteRepository, NoteRepository>();
      services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();
      services.AddScoped<IStudentCoursePerformanceSummaryRepository, StudentCoursePerformanceSummaryRepository>();
      services.AddScoped<IStudentFlashcardRepository, StudentFlashcardRepository>();
      services.AddScoped<IStudentFlashcardAnalyticsRepository, StudentFlashcardAnalyticsRepository>();
      services.AddScoped<IStudentOverallPerformanceSummaryRepository, StudentOverallPerformanceSummaryRepository>();
      services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();
      services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }

  private static void ConfigureMapster()
  {
    var config = TypeAdapterConfig.GlobalSettings;
    config.Scan(typeof(DependencyInjection).Assembly); // scans EducAIte.Application
  }
}
