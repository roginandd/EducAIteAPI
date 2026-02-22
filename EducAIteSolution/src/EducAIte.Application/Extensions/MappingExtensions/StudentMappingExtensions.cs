using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity.Data;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class StudentMappingExtensions
{
    public static StudentBriefResponse ToBriefDTO(this Student student) => student.BuildAdapter().AddParameters("FullName", $"{student.FirstName} {student.LastName}").Adapt<StudentBriefResponse>();

    public static StudentResponse ToDTO(this Student student) => student.Adapt<StudentResponse>();

    public static Student toEntity(this StudentRegistrationRequest request)
    {
        var student = request.Adapt<Student>();
        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        student.CreatedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        return student;
    }

    public static StudentProfileResponse ToStudentProfile(this Student student) => student.Adapt<StudentProfileResponse>();
}