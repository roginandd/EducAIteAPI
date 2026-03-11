using EducAIte.Infrastructure.Data;
using EducAIte.Domain.Interfaces;
using EducAIte.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducAIte.Infrastructure.Repositories
{
    public class FileMetadataRepository(ApplicationDbContext context) : IFileMetadataRepository
    {
        
    }
}