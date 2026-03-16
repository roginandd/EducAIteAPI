using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class StudentMappingExtensions
{
    public static StudentBriefResponse ToBriefDTO(this Student student, ISqidService sqidService)
    {
        return student
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AddParameters("FullName", $"{student.FirstName} {student.LastName}")
            .AdaptToType<StudentBriefResponse>();
    }

    public static StudentResponse ToDTO(this Student student, ISqidService sqidService)
    {
        return student
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudentResponse>();
    }

    public static Student toEntity(this StudentRegistrationRequest request)
    {
        var student = request.Adapt<Student>();
        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        student.CreatedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        return student;
    }

    public static StudentProfileResponse ToStudentProfile(this Student student, ISqidService sqidService)
    {
        return student
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<StudentProfileResponse>();
    }
}
