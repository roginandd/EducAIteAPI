namespace EducAIte.Application.DTOs.Request;

public record CreateCourseRequest
(
    string EDPCode,
    string CourseName,
    byte Units
);