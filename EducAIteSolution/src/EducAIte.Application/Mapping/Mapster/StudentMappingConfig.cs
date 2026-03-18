namespace EducAIte.Application.Mapping.Configurations;

using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

public sealed class StudentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Student, StudentBriefResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.FullName, src => GetFullName())
            .Map(dest => dest.Program, src => src.Program.ToString());

        config.NewConfig<Student, StudentResponse>()
            .Map(dest => dest.Sqid, src => GetSqidService().Encode(src.StudentId))
            .Map(dest => dest.Id, src => src.StudentId)
            .Map(dest => dest.Program, src => src.Program.ToString());

        config.NewConfig<Student, StudentProfileResponse>()
            .Map(dest => dest.StudentSqid, src => src.StudentId)
            .Map(dest => dest.StudentSqid, src => GetSqidService().Encode(src.StudentId));
    }

    private static ISqidService GetSqidService()
    {
        if (MapContext.Current?.Parameters.TryGetValue("sqidService", out object? value) != true || value is not ISqidService sqidService)
        {
            throw new InvalidOperationException("sqidService mapping parameter is required.");
        }

        return sqidService;
    }

    private static string GetFullName()
    {
        if (MapContext.Current?.Parameters.TryGetValue("FullName", out object? value) != true || value is not string fullName)
        {
            throw new InvalidOperationException("FullName mapping parameter is required.");
        }

        return fullName;
    }
}
