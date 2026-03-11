using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Application.Services;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Repositories;
using Mapster;

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
        services.AddScoped<IFlashcardService, FlashcardService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<INoteOrderingService, NoteOrderingService>();
        services.AddScoped<IResourceOwnershipService, ResourceOwnershipService>();
        services.AddSingleton<ISqidService, SqidService>();
        services.AddScoped<IStudentFlashcardService, StudentFlashcardService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IStudyLoadService, StudyLoadService>();

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IFlashcardRepository, FlashcardRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IStudentFlashcardRepository, StudentFlashcardRepository>();
        services.AddScoped<IStudyLoadRepository, StudyLoadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
      }

      private static void ConfigureMapster()
      {
          var config = TypeAdapterConfig.GlobalSettings;
          config.Scan(typeof(DependencyInjection).Assembly); // scans EducAIte.Application
      }
  }
