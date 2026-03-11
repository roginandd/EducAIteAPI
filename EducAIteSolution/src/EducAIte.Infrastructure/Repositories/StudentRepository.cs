using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context) => _context = context;


    public async Task<Student?> GetByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        Student? student = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentId == studentId, cancellationToken);

        if (student == null)
           return null;

        return student;
    }

    public async Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber, CancellationToken cancellationToken = default)
    {
        Student? student = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentIdNumber == studentIdNumber, cancellationToken);

        if (student == null)
            return null;
        
        return student;
    }

    public async Task<Student> AddStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
        return student;
    }
    

}
