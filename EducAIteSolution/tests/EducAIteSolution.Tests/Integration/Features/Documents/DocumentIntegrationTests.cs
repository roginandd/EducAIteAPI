using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EducAIteSolution.Tests.Integration.Features.Documents;

public class DocumentIntegrationTests : IntegrationTestBase
{
    private readonly IDocumentService _documentService;
    private readonly ITestOutputHelper _outputHelper;

    public DocumentIntegrationTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _outputHelper = outputHelper;
        _documentService = ServiceProvider.GetRequiredService<IDocumentService>();
    }

    [Fact]
    public async Task CreateDocumentAsync_WithValidRequest_PersistsDocument()
    {
        var student = await SeedStudentAsync("2026001");
        var folder = await SeedFolderAsync(student.StudentId, "syllabus");
        var file = await SeedFileMetadataAsync(student.StudentId, "syllabus.pdf", "files/syllabus.pdf");

        var result = await _documentService.CreateDocumentAsync(new CreateDocumentRequest
        {
            DocumentName = "Syllabus",
            FolderId = folder.FolderId,
            FileMetadataId = file.FileMetaDataId
        });

        var persistedDocument = await ApplicationDbContext.Documents
            .Include(document => document.Folder)
            .FirstOrDefaultAsync(document => document.DocumentId == result.DocumentId);

        Assert.NotNull(persistedDocument);
        Assert.Equal("Syllabus", persistedDocument.DocumentName);
        Assert.Equal(student.StudentId, persistedDocument.Folder.StudentId);
        Assert.Equal(student.StudentId, result.StudentId);

        await DumpDatabaseStateAsync(nameof(CreateDocumentAsync_WithValidRequest_PersistsDocument));
    }

    [Fact]
    public async Task GetDocumentsByStudentAsync_ReturnsOnlyNonDeletedDocuments()
    {
        var student = await SeedStudentAsync("2026002");
        var folder = await SeedFolderAsync(student.StudentId, "notes");
        var fileOne = await SeedFileMetadataAsync(student.StudentId, "notes-1.pdf", "files/notes-1.pdf");
        var fileTwo = await SeedFileMetadataAsync(student.StudentId, "notes-2.pdf", "files/notes-2.pdf");

        await SeedDocumentAsync(folder.FolderId, fileOne.FileMetaDataId, "Active Notes");
        await SeedDocumentAsync(folder.FolderId, fileTwo.FileMetaDataId, "Old Notes", isDeleted: true);

        var results = (await _documentService.GetDocumentsByStudentAsync(student.StudentId)).ToList();

        Assert.Single(results);
        Assert.Equal("Active Notes", results[0].DocumentName);

        await DumpDatabaseStateAsync(nameof(GetDocumentsByStudentAsync_ReturnsOnlyNonDeletedDocuments));
    }

    [Fact]
    public async Task UpdateDocumentAsync_WithNewValues_UpdatesPersistedDocument()
    {
        var student = await SeedStudentAsync("2026003");
        var folder = await SeedFolderAsync(student.StudentId, "reviewers");
        var replacementFolder = await SeedFolderAsync(student.StudentId, "reviewers-2");
        var originalFile = await SeedFileMetadataAsync(student.StudentId, "reviewer-v1.pdf", "files/reviewer-v1.pdf");
        var replacementFile = await SeedFileMetadataAsync(student.StudentId, "reviewer-v2.pdf", "files/reviewer-v2.pdf");
        var document = await SeedDocumentAsync(folder.FolderId, originalFile.FileMetaDataId, "Reviewer");

        var updated = await _documentService.UpdateDocumentAsync(document.DocumentId, new UpdateDocumentRequest
        {
            DocumentName = "Reviewer Final",
            FolderId = replacementFolder.FolderId,
            FileMetadataId = replacementFile.FileMetaDataId
        });

        var persistedDocument = await ApplicationDbContext.Documents
            .FirstAsync(savedDocument => savedDocument.DocumentId == document.DocumentId);

        Assert.True(updated);
        Assert.Equal("Reviewer Final", persistedDocument.DocumentName);
        Assert.Equal(replacementFolder.FolderId, persistedDocument.FolderId);
        Assert.Equal(replacementFile.FileMetaDataId, persistedDocument.FileMetadataId);

        await DumpDatabaseStateAsync(nameof(UpdateDocumentAsync_WithNewValues_UpdatesPersistedDocument));
    }

    [Fact]
    public async Task DeleteDocumentAsync_SoftDeletesDocument()
    {
        var student = await SeedStudentAsync("2026004");
        var folder = await SeedFolderAsync(student.StudentId, "archive");
        var file = await SeedFileMetadataAsync(student.StudentId, "archive.pdf", "files/archive.pdf");
        var document = await SeedDocumentAsync(folder.FolderId, file.FileMetaDataId, "Archive Copy");

        var deleted = await _documentService.DeleteDocumentAsync(document.DocumentId);
        var persistedDocument = await ApplicationDbContext.Documents
            .IgnoreQueryFilters()
            .FirstAsync(savedDocument => savedDocument.DocumentId == document.DocumentId);
        var fetchedDocument = await _documentService.GetDocumentByIdAsync(document.DocumentId);

        Assert.True(deleted);
        Assert.True(persistedDocument.IsDeleted);
        Assert.Null(fetchedDocument);

        await DumpDatabaseStateAsync(nameof(DeleteDocumentAsync_SoftDeletesDocument));
    }

    private async Task<Student> SeedStudentAsync(string studentIdNumber)
    {
        var student = new Student
        {
            StudentIdNumber = studentIdNumber,
            Program = StudentPrograms.InformationTechnology,
            BirthDate = new DateTime(2000, 1, 1),
            Semester = 1,
            FirstName = "Test",
            LastName = "Student",
            MiddleName = string.Empty,
            PasswordHash = "hashed-password",
            Email = $"{studentIdNumber}@example.com",
            PhoneNumber = "09123456789"
        };

        ApplicationDbContext.Students.Add(student);
        await ApplicationDbContext.SaveChangesAsync();

        return student;
    }

    private async Task<Folder> SeedFolderAsync(long studentId, string folderKey)
    {
        var folder = new Folder
        {
            StudentId = studentId,
            SchoolYear = new SchoolYear(2025, 2026),
            Semester = 1,
            FolderKey = folderKey,
            Name = folderKey
        };

        ApplicationDbContext.Folders.Add(folder);
        await ApplicationDbContext.SaveChangesAsync();

        return folder;
    }

    private async Task<FileMetadata> SeedFileMetadataAsync(long studentId, string fileName, string storageKey)
    {
        var fileMetadata = new FileMetadata
        {
            FileName = fileName,
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            StorageKey = storageKey,
            FileSizeInBytes = 4096,
            StudentId = studentId
        };

        ApplicationDbContext.FileMetadata.Add(fileMetadata);
        await ApplicationDbContext.SaveChangesAsync();

        return fileMetadata;
    }

    private async Task<Document> SeedDocumentAsync(long folderId, long fileMetadataId, string documentName, bool isDeleted = false)
    {
        var document = new Document(documentName, folderId, fileMetadataId);
        if (isDeleted)
        {
            document.MarkDeleted();
        }

        ApplicationDbContext.Documents.Add(document);
        await ApplicationDbContext.SaveChangesAsync();

        return document;
    }

    private async Task DumpDatabaseStateAsync(string testName)
    {
        List<Student> students = await ApplicationDbContext.Students.AsNoTracking().ToListAsync();
        List<Folder> folders = await ApplicationDbContext.Folders.AsNoTracking().ToListAsync();
        List<FileMetadata> files = await ApplicationDbContext.FileMetadata.AsNoTracking().ToListAsync();
        List<Document> documents = await ApplicationDbContext.Documents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        _outputHelper.WriteLine($"=== DB Snapshot: {testName} ===");
        _outputHelper.WriteLine($"Students: {students.Count}, Folders: {folders.Count}, Files: {files.Count}, Documents(all): {documents.Count}");

        foreach (Document document in documents)
        {
            _outputHelper.WriteLine(
                $"DocumentId={document.DocumentId}, Name={document.DocumentName}, FolderId={document.FolderId}, FileMetadataId={document.FileMetadataId}, IsDeleted={document.IsDeleted}");
        }
    }
}
