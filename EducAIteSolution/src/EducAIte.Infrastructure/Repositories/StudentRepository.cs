using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context) => _context = context;


    public async Task<Student?> GetByStudentIdAsync(long studentId)
    {
        Student? student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId);

        if (student == null)
           return null;

        return student;
    }

    public async Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber)
    {
        Student? student = await _context.Students.FirstOrDefaultAsync(student => student.StudentIdNumber == studentIdNumber);

        if (student == null)
            return null;
        
        return student;
    }

    public async Task<Student> AddStudentAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        
        await _context.SaveChangesAsync();
        return student;
    }
    

}