using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace EducAIteSolution.Tests.Unit.Features.StudyLoads;

public class StudyLoadRequestMappingTests
{
    [Fact]
    public void ToEntity_CreateRequest_MapsStudyLoadFields()
    {
        var request = new StudyLoadCreateRequest
        {
            StudentSqid = "student-sqid",
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = (int)Semester.First,
            StudyLoadDocument = BuildFormFile("study-load.pdf", "application/pdf", 1024)
        };

        StudyLoad result = request.ToEntity(7);

        Assert.Equal(7, result.StudentId);
        Assert.Equal(2025, result.SchoolYearStart);
        Assert.Equal(2026, result.SchoolYearEnd);
        Assert.Equal(Semester.First, result.Semester);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public void ApplyToEntity_UpdateRequest_MapsStudyLoadFields()
    {
        var request = new StudyLoadUpdateRequest
        {
            StudentSqid = "student-sqid",
            StudyLoadSqid = "study-load-sqid",
            SchoolYearStart = 2026,
            SchoolYearEnd = 2027,
            Semester = (int)Semester.Second,
            StudyLoadDocument = BuildFormFile("updated-study-load.pdf", "application/pdf", 2048)
        };

        var studyLoad = new StudyLoad
        {
            StudentId = 7,
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = Semester.First,
            UpdatedAt = new DateTime(2026, 3, 24, 1, 0, 0, DateTimeKind.Utc)
        };

        request.ApplyToEntity(studyLoad);

        Assert.Equal(2026, studyLoad.SchoolYearStart);
        Assert.Equal(2027, studyLoad.SchoolYearEnd);
        Assert.Equal(Semester.Second, studyLoad.Semester);
        Assert.True(studyLoad.UpdatedAt > new DateTime(2026, 3, 24, 1, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ToFileMetadataEntity_CreateRequest_MapsFileMetadataFields()
    {
        var request = new StudyLoadCreateRequest
        {
            StudentSqid = "student-sqid",
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = (int)Semester.First,
            StudyLoadDocument = BuildFormFile("study-load.pdf", "application/pdf", 1024)
        };

        FileMetadata result = request.ToFileMetadataEntity(7, "users/7/study-loads/file.pdf");

        Assert.Equal("study-load.pdf", result.FileName);
        Assert.Equal(".pdf", result.FileExtension);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("users/7/study-loads/file.pdf", result.StorageKey);
        Assert.Equal(1024, result.FileSizeInBytes);
        Assert.Equal(7, result.StudentId);
    }

    [Fact]
    public void ApplyToFileMetadataEntity_UpdateRequest_MapsFileMetadataFields()
    {
        var request = new StudyLoadUpdateRequest
        {
            StudentSqid = "student-sqid",
            StudyLoadSqid = "study-load-sqid",
            SchoolYearStart = 2026,
            SchoolYearEnd = 2027,
            Semester = (int)Semester.Second,
            StudyLoadDocument = BuildFormFile("updated-study-load.pdf", "application/pdf", 2048)
        };

        var fileMetadata = new FileMetadata
        {
            FileName = "old.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            StorageKey = "users/7/old.pdf",
            FileSizeInBytes = 512,
            StudentId = 7,
            UpdatedAt = new DateTime(2026, 3, 24, 2, 0, 0, DateTimeKind.Utc)
        };

        request.ApplyToFileMetadataEntity(fileMetadata, "users/7/updated-study-load.pdf");

        Assert.Equal("updated-study-load.pdf", fileMetadata.FileName);
        Assert.Equal(".pdf", fileMetadata.FileExtension);
        Assert.Equal("application/pdf", fileMetadata.ContentType);
        Assert.Equal("users/7/updated-study-load.pdf", fileMetadata.StorageKey);
        Assert.Equal(2048, fileMetadata.FileSizeInBytes);
        Assert.True(fileMetadata.UpdatedAt > new DateTime(2026, 3, 24, 2, 0, 0, DateTimeKind.Utc));
    }

    private static IFormFile BuildFormFile(string fileName, string contentType, int length)
    {
        var stream = new MemoryStream(new byte[length]);
        return new FormFile(stream, 0, length, "studyLoadDocument", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
