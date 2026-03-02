namespace EducAIte.Application.DTOs.Response;



public record CourseResponse
(
    long CourseId,
    string EDPCode,
    string CourseName,
    byte Units
);