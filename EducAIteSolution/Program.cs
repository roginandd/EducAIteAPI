using EducAIte.Application.Extensions;
using EducAIte.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiWithBearerAuth();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();


builder.Services.AddApplicationLayer(); // extension method to register app services and configure Mapster

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAwsServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapScalarWithAuth();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        (int statusCode, string title) = exception switch
        {
            ArgumentException => (400, "Bad Request"),
            KeyNotFoundException => (404, "Resource Not Found"),
            InvalidOperationException => (409, "Conflict"),
            DbUpdateConcurrencyException => (409, "Concurrency Conflict"),
            _ => (500, "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception?.Message
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.MapGet("/", () => "Hello World!");


app.Run();
