using EducAIte.Application.Interfaces;
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

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();

        return services;
      }

      private static void ConfigureMapster()
      {
          var config = TypeAdapterConfig.GlobalSettings;
          config.Scan(typeof(DependencyInjection).Assembly); // scans EducAIte.Application
      }
  }
