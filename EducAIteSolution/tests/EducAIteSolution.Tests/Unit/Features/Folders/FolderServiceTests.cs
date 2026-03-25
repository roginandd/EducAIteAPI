using EducAIte.Application.DTOs.Request;
using EducAIte.Application.Extensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Implementation;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using EducAIte.Domain.ValueObjects;
using Mapster;
using Microsoft.Extensions.Logging;
using Moq;

namespace EducAIteSolution.Tests.Unit.Features.Folders;

public class FolderServiceTests
{
    private readonly Mock<IFolderRepository> _folderRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IResourceOwnershipService> _resourceOwnershipServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<FolderService>> _loggerMock;
    private readonly ISqidService _sqidService;
    private readonly FolderService _folderService;

    static FolderServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
    }

    public FolderServiceTests()
    {
        _folderRepositoryMock = new Mock<IFolderRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _resourceOwnershipServiceMock = new Mock<IResourceOwnershipService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<FolderService>>();
        _sqidService = new SqidService();

        _folderService = new FolderService(
            _folderRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _resourceOwnershipServiceMock.Object,
            _sqidService,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateFolderAsync_WithValidRequest_ReturnsCreatedFolder()
    {
        CreateFolderRequest request = new()
        {
            FolderKey = " reviewer-2026 ",
            Name = " Reviewers ",
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = 2,
            CourseId = 8
        };

        _folderRepositoryMock
            .Setup(repository => repository.FolderKeyExistsAsync(7, "reviewer-2026", null, default))
            .ReturnsAsync(false);
        _courseRepositoryMock
            .Setup(repository => repository.GetByIdAsync(8, default))
            .ReturnsAsync(new Course { CourseId = 8, EDPCode = "EDP-101", CourseName = "Physics", Units = 3 });
        _folderRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Folder>(), default))
            .Callback<Folder, CancellationToken>((folder, _) => folder.FolderId = 11);
        _folderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(11, default))
            .ReturnsAsync(new Folder(7, new SchoolYear(2025, 2026), 2, "reviewer-2026", "Reviewers", 8)
            {
                FolderId = 11
            });

        var result = await _folderService.CreateFolderAsync(request, 7);

        Assert.Equal(_sqidService.Encode(11), result.Sqid);
        Assert.Equal("Reviewers", result.Name);
        Assert.Equal("reviewer-2026", result.FolderKey);
        Assert.Equal(_sqidService.Encode(8), result.CourseSqid);
    }

    [Fact]
    public async Task UpdateFolderAsync_WhenMovingIntoDescendant_ThrowsInvalidOperationException()
    {
        long folderId = 10;
        long studentId = 7;
        long childFolderId = 12;

        UpdateFolderRequest request = new()
        {
            FolderKey = "reviewers",
            Name = "Reviewers",
            SchoolYearStart = 2025,
            SchoolYearEnd = 2026,
            Semester = 1,
            ParentFolderSqid = _sqidService.Encode(childFolderId)
        };

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureFolderOwnedByStudentAsync(folderId, studentId, default))
            .Returns(Task.CompletedTask);
        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureFolderOwnedByStudentAsync(childFolderId, studentId, default))
            .Returns(Task.CompletedTask);
        _folderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(folderId, default))
            .ReturnsAsync(new Folder(studentId, new SchoolYear(2025, 2026), 1, "root-reviewers", "Reviewers")
            {
                FolderId = folderId
            });
        _folderRepositoryMock
            .Setup(repository => repository.FolderKeyExistsAsync(studentId, "reviewers", folderId, default))
            .ReturnsAsync(false);
        _folderRepositoryMock
            .Setup(repository => repository.IsDescendantAsync(folderId, childFolderId, default))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _folderService.UpdateFolderAsync(_sqidService.Encode(folderId), request, studentId));
    }

    [Fact]
    public async Task GetFoldersByStudentAsync_WithMissingStudent_ThrowsKeyNotFoundException()
    {
        _studentRepositoryMock
            .Setup(repository => repository.GetByStudentIdAsync(9, default))
            .ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _folderService.GetFoldersByStudentAsync(9));
    }

    [Fact]
    public async Task GetFoldersBySemesterAsync_WithValidSemester_ReturnsFilteredFolders()
    {
        _studentRepositoryMock
            .Setup(repository => repository.GetByStudentIdAsync(7, default))
            .ReturnsAsync(new Student { StudentId = 7, StudentIdNumber = "2025-0001", FirstName = "A", LastName = "B", Email = "a@b.com" });
        _folderRepositoryMock
            .Setup(repository => repository.GetAllByStudentIdAndSemesterAsync(7, 2, default))
            .ReturnsAsync([
                new Folder(7, new SchoolYear(2025, 2026), 2, "reviewers", "Reviewers") { FolderId = 5 }
            ]);

        IReadOnlyList<EducAIte.Application.DTOs.Response.FolderResponse> result =
            await _folderService.GetFoldersBySemesterAsync(7, 2);

        Assert.Single(result);
        Assert.Equal((byte)2, result[0].Semester);
    }

    [Fact]
    public async Task GetSubFoldersAsync_WithOwnedParent_ReturnsChildren()
    {
        long parentFolderId = 4;
        long studentId = 7;

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureFolderOwnedByStudentAsync(parentFolderId, studentId, default))
            .Returns(Task.CompletedTask);
        _folderRepositoryMock
            .Setup(repository => repository.GetSubFoldersAsync(parentFolderId, default))
            .ReturnsAsync([
                new Folder(studentId, new SchoolYear(2025, 2026), 1, "child", "Child Folder", parentFolderId: parentFolderId) { FolderId = 6 }
            ]);

        IReadOnlyList<EducAIte.Application.DTOs.Response.FolderResponse> result =
            await _folderService.GetSubFoldersAsync(_sqidService.Encode(parentFolderId), studentId);

        Assert.Single(result);
        Assert.Equal(_sqidService.Encode(parentFolderId), result[0].ParentFolderSqid);
    }

    [Fact]
    public async Task GetParentFolderAsync_WithParentFolder_ReturnsParent()
    {
        long folderId = 9;
        long parentFolderId = 3;
        long studentId = 7;

        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureFolderOwnedByStudentAsync(folderId, studentId, default))
            .Returns(Task.CompletedTask);
        _resourceOwnershipServiceMock
            .Setup(service => service.EnsureFolderOwnedByStudentAsync(parentFolderId, studentId, default))
            .Returns(Task.CompletedTask);
        _folderRepositoryMock
            .Setup(repository => repository.GetParentFolderAsync(folderId, default))
            .ReturnsAsync(new Folder(studentId, new SchoolYear(2025, 2026), 1, "parent", "Parent Folder")
            {
                FolderId = parentFolderId
            });

        var result = await _folderService.GetParentFolderAsync(_sqidService.Encode(folderId), studentId);

        Assert.NotNull(result);
        Assert.Equal(_sqidService.Encode(parentFolderId), result!.Sqid);
    }
}
