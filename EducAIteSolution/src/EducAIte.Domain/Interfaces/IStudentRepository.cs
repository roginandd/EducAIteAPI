using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentRepository
{
    public Task<Student?> GetByStudentIdAsync(long studentId);
    public Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber);
    public Task<Student> AddStudentAsync(Student student);
}