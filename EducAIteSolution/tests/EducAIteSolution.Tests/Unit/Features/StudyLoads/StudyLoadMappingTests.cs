using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using Mapster;

namespace EducAIteSolution.Tests.Unit.Features.StudyLoads;

public class StudyLoadMappingTests
{
    private readonly ISqidService _sqidService = new SqidService();

    static StudyLoadMappingTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    [Fact]
    public void ToDto_WithNestedFileMetadata_MapsExpectedFields()
    {
        var studyLoad = BuildStudyLoad();

        StudyLoadDto result = studyLoad.ToDto(_sqidService);

        Assert.Equal(_sqidService.Encode(studyLoad.StudyLoadId), result.Sqid);
        Assert.Equal(_sqidService.Encode(studyLoad.StudentId), result.StudentSqid);
        Assert.Equal(_sqidService.Encode(studyLoad.FileMetadataId), result.FileMetadataSqid);
        Assert.Equal(studyLoad.SchoolYearStart.ToString(), result.SchoolYearStart);
        Assert.Equal(studyLoad.SchoolYearEnd.ToString(), result.SchoolYearEnd);
        Assert.Equal(studyLoad.Semester.ToString(), result.Semester);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(studyLoad.FileMetadata.FileMetaDataId, result.FileMetadata!.FileMetadataId);
        Assert.Equal(_sqidService.Encode(studyLoad.FileMetadata.FileMetaDataId), result.FileMetadata.Sqid);
        Assert.Equal(_sqidService.Encode(studyLoad.FileMetadata.StudentId), result.FileMetadata.StudentSqid);
        Assert.Single(result.Courses);
        Assert.Equal("Discrete Mathematics", result.Courses.Single().CourseName);
    }

    [Fact]
    public void ToResponse_WithNestedFileMetadata_MapsExpectedFields()
    {
        var studyLoad = BuildStudyLoad();

        StudyLoadResponse result = studyLoad.ToResponse(_sqidService);

        Assert.Equal(_sqidService.Encode(studyLoad.StudyLoadId), result.Sqid);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(studyLoad.FileMetadata.FileMetaDataId, result.FileMetadata!.FileMetadataId);
        Assert.Equal("First", result.Semester);
        Assert.Equal(3, result.TotalUnits);
    }

    private static StudyLoad BuildStudyLoad()
    {
        return new StudyLoad
        {
            StudyLoadId = 44,
            StudentId = 7,
            FileMetadataId = 12,
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = Semester.First,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FileMetadata = new FileMetadata
            {
                FileMetaDataId = 12,
                StudentId = 7,
                FileName = "study-load.pdf",
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                StorageKey = "users/7/study-loads/2025-2026/1/study-load.pdf",
                FileSizeInBytes = 1024,
                UploadedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Courses =
            [
                new Course
                {
                    CourseId = 3,
                    EDPCode = "CS101",
                    CourseName = "Discrete Mathematics",
                    Units = 3
                }
            ]
        };
    }
}
