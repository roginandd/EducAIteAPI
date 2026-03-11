using EducAIte.Domain.Entities;

namespace EducAIte.Domain.Interfaces;

public interface IStudentRepository
{
    public Task<Student?> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default);
    public Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber, CancellationToken cancellationToken = default);
    public Task<Student> AddStudentAsync(Student student, CancellationToken cancellationToken = default);
}