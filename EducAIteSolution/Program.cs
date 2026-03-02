using System.Text;
using Amazon;
using Amazon.S3;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using EducAIte.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// CloudFront / AWS
builder.Services.AddScoped<IAWSService>(sp =>
{ 
    var region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "ap-southeast-2");
    var accessKey = builder.Configuration["AWS:AccessKey"]!;
    var secretKey = builder.Configuration["AWS:SecretKey"]!;
    var s3Client = new AmazonS3Client(accessKey, secretKey, region);
    var domain = builder.Configuration["CloudFront:Domain"]!;
    var keyPairId = builder.Configuration["CloudFront:KeyPairId"]!;
    var privateKeyPath = builder.Configuration["CloudFront:PrivateKeyPath"]!;
    var privateKey = File.ReadAllText(privateKeyPath);
    var bucketName = builder.Configuration["AWS:BucketName"]!;
    return new AWSService(s3Client, bucketName, domain, keyPairId, privateKey);
});

// Business Logic and Application Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at app root
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.MapControllers();



app.MapGet("/", () => "Hello World!");


app.Run();
