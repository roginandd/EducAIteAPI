using EducAIte.Application.Interfaces;
using EducAIte.Application.Services;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using EducAIte.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EducAIteSolution.Tests;


public abstract class IntegrationTestBase : IDisposable
{
    protected readonly ApplicationDbContext ApplicationDbContext;
    protected readonly IServiceProvider ServiceProvider;    
    private readonly SqliteConnection _connection;

    protected IntegrationTestBase(ITestOutputHelper outputHelper)
    {
        _connection = new SqliteConnection("Datasource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        // 1. Add Configuration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "supersecretkeythatislongenoughforhmacsha256",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryInMinutes"] = "60"
            })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        // 2. Add Logging Services
        services.AddLogging(builder => 
        {
            builder.AddConsole(); // Optional: allows seeing logs in the console
            builder.AddDebug();
        });

        // 3. Add Database and Services
        services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlite(_connection)
        );

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentRepository, StudentRepository>();

        // 4. Build and Assign
        ServiceProvider = services.BuildServiceProvider();
        
        // Use the ServiceProvider to create the logger

        ApplicationDbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ApplicationDbContext.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Close();
}