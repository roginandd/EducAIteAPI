using System.Text;
using Amazon;
using Amazon.S3;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EducAIte.Application.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAWSService>(_ =>
        {
            var regionName = configuration["AWS:Region"] ?? "ap-southeast-2";
            var accessKey = GetRequiredConfig(configuration, "AWS:AccessKey");
            var secretKey = GetRequiredConfig(configuration, "AWS:SecretKey");
            var bucketName = GetRequiredConfig(configuration, "AWS:BucketName");

            var cloudFrontDomain = GetRequiredConfig(configuration, "CloudFront:Domain");
            var cloudFrontKeyPairId = GetRequiredConfig(configuration, "CloudFront:KeyPairId");
            var cloudFrontPrivateKeyPath = GetRequiredConfig(configuration, "CloudFront:PrivateKeyPath");

            if (!File.Exists(cloudFrontPrivateKeyPath))
            {
                throw new InvalidOperationException($"CloudFront private key file not found: {cloudFrontPrivateKeyPath}");
            }

            var cloudFrontPrivateKey = File.ReadAllText(cloudFrontPrivateKeyPath);
            var region = RegionEndpoint.GetBySystemName(regionName);
            var s3Client = new AmazonS3Client(accessKey, secretKey, region);

            return new AWSService(s3Client, bucketName, cloudFrontDomain, cloudFrontKeyPairId, cloudFrontPrivateKey);
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtIssuer = GetRequiredConfig(configuration, "Jwt:Issuer");
        var jwtAudience = GetRequiredConfig(configuration, "Jwt:Audience");
        var jwtKey = GetRequiredConfig(configuration, "Jwt:Key");

        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters for HMAC-SHA256.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        return services;
    }

    private static string GetRequiredConfig(IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Missing configuration key: {key}");
    }
}
