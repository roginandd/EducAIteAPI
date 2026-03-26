using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FolderService> _logger;

    public FolderService(
        IFolderRepository folderRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IDocumentRepository documentRepository,
        INoteRepository noteRepository,
        IResourceOwnershipService resourceOwnershipService,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<FolderService> logger)
    {
        _folderRepository = folderRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _documentRepository = documentRepository;
        _noteRepository = noteRepository;
        _resourceOwnershipService = resourceOwnershipService;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FolderResponse?> GetFolderByIdAsync(string sqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long folderId))
        {
            return null;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);
        Folder? folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);

        return folder?.ToResponse(_sqidService);
    }

    public async Task<FolderContentsResponse?> GetContentsAsync(
        string sqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long folderId))
        {
            return null;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);

        Folder? folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
        {
            return null;
        }

        IReadOnlyList<Folder> subFolders = await _folderRepository.GetSubFoldersAsync(folderId, cancellationToken);
        IReadOnlyList<Document> documents = await _documentRepository.GetAllByFolderIdAndStudentIdAsync(folderId, studentId, cancellationToken);
        IReadOnlyList<Note> notes = await _noteRepository.GetAllByDocumentIdsAndStudentIdAsync(
            documents.Select(document => document.DocumentId).ToArray(),
            studentId,
            cancellationToken);

        return new FolderContentsResponse
        {
            Folder = folder.ToResponse(_sqidService),
            SubFolders = subFolders.Select(ToContentItemResponse).ToList(),
            Documents = documents.Select(ToContentItemResponse).ToList(),
            Notes = notes.Select(ToContentItemResponse).ToList()
        };
    }

    public async Task<IReadOnlyList<FolderResponse>> GetFoldersByStudentAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);

        IReadOnlyList<Folder> folders = await _folderRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        return folders.Select(folder => folder.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<FolderResponse>> GetFoldersBySemesterAsync(
        long studentId,
        byte semester,
        CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);
        EnsureSemesterIsValid(semester);

        IReadOnlyList<Folder> folders = await _folderRepository.GetAllByStudentIdAndSemesterAsync(
            studentId,
            semester,
            cancellationToken);

        return folders.Select(folder => folder.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<FolderResponse>> GetFoldersBySchoolYearAsync(
        long studentId,
        int schoolYearStart,
        int schoolYearEnd,
        CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);
        EnsureSchoolYearIsValid(schoolYearStart, schoolYearEnd);

        IReadOnlyList<Folder> folders = await _folderRepository.GetAllByStudentIdAndSchoolYearAsync(
            studentId,
            schoolYearStart,
            schoolYearEnd,
            cancellationToken);

        return folders.Select(folder => folder.ToResponse(_sqidService)).ToList();
    }

    public async Task<IReadOnlyList<FolderResponse>> GetSubFoldersAsync(
        string parentFolderSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(parentFolderSqid, out long parentFolderId))
        {
            return [];
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(parentFolderId, studentId, cancellationToken);

        IReadOnlyList<Folder> folders = await _folderRepository.GetSubFoldersAsync(parentFolderId, cancellationToken);
        return folders.Select(folder => folder.ToResponse(_sqidService)).ToList();
    }

    public async Task<FolderResponse?> GetParentFolderAsync(
        string folderSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(folderSqid, out long folderId))
        {
            return null;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);

        Folder? parentFolder = await _folderRepository.GetParentFolderAsync(folderId, cancellationToken);
        if (parentFolder is null)
        {
            return null;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(parentFolder.FolderId, studentId, cancellationToken);
        return parentFolder.ToResponse(_sqidService);
    }

    public async Task<FolderResponse> CreateFolderAsync(CreateFolderRequest request, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        long? parentFolderId = DecodeOptionalSqid(request.ParentFolderSqid, "ParentFolderSqid");
        await ValidateFolderRequestAsync(request.FolderKey, request.CourseId, parentFolderId, studentId, null, cancellationToken);

        Folder folder = request.ToEntity(studentId);
        folder.ParentFolderId = parentFolderId;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _folderRepository.AddAsync(folder, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating folder for student {StudentId}", studentId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        Folder? createdFolder = await _folderRepository.GetByIdAsync(folder.FolderId, cancellationToken);
        _logger.LogInformation("Created folder {FolderId}", folder.FolderId);

        if (createdFolder is null)
        {
            throw new InvalidOperationException("Folder was created but could not be reloaded.");
        }

        return createdFolder.ToResponse(_sqidService);
    }

    public async Task<bool> UpdateFolderAsync(
        string sqid,
        UpdateFolderRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long folderId))
        {
            return false;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);
        Folder? existingFolder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (existingFolder is null)
        {
            return false;
        }

        long? parentFolderId = DecodeOptionalSqid(request.ParentFolderSqid, "ParentFolderSqid");

        if (IsUnchanged(existingFolder, request, parentFolderId))
        {
            return true;
        }

        await ValidateFolderRequestAsync(request.FolderKey, request.CourseId, parentFolderId, studentId, folderId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            request.ApplyToEntity(existingFolder);
            existingFolder.ParentFolderId = parentFolderId;
            await _folderRepository.UpdateAsync(existingFolder, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating folder {FolderSqid}", sqid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation("Updated folder {FolderSqid}", sqid);
        return true;
    }

    public async Task<bool> DeleteFolderAsync(string sqid, long studentId, CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long folderId))
        {
            return false;
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);
        Folder? existingFolder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (existingFolder is null)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _folderRepository.DeleteAsync(folderId, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting folder {FolderSqid}", sqid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation("Deleted folder {FolderSqid}", sqid);
        return true;
    }

    private async Task ValidateFolderRequestAsync(
        string folderKey,
        long? courseId,
        long? parentFolderId,
        long studentId,
        long? currentFolderId,
        CancellationToken cancellationToken)
    {
        if (await _folderRepository.FolderKeyExistsAsync(studentId, folderKey.Trim(), currentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Folder with key {folderKey.Trim()} already exists for this student.");
        }

        if (courseId.HasValue)
        {
            Course? course = await _courseRepository.GetByIdAsync(courseId.Value, cancellationToken);
            if (course is null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId.Value} not found.");
            }
        }

        if (!parentFolderId.HasValue)
        {
            return;
        }

        if (currentFolderId.HasValue && parentFolderId.Value == currentFolderId.Value)
        {
            throw new InvalidOperationException("A folder cannot be its own parent.");
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(parentFolderId.Value, studentId, cancellationToken);

        if (currentFolderId.HasValue &&
            await _folderRepository.IsDescendantAsync(currentFolderId.Value, parentFolderId.Value, cancellationToken))
        {
            throw new InvalidOperationException("A folder cannot be moved inside one of its descendants.");
        }
    }

    private async Task EnsureStudentExistsAsync(long studentId, CancellationToken cancellationToken)
    {
        EnsureStudentIdIsValid(studentId);

        Student? student = await _studentRepository.GetByStudentIdAsync(studentId, cancellationToken);
        if (student is null || student.IsDeleted)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }
    }

    private static bool IsUnchanged(Folder existingFolder, UpdateFolderRequest request, long? parentFolderId)
    {
        return string.Equals(existingFolder.FolderKey, request.FolderKey.Trim(), StringComparison.Ordinal) &&
               string.Equals(existingFolder.Name, request.Name.Trim(), StringComparison.Ordinal) &&
               existingFolder.SchoolYear.StartYear == request.SchoolYearStart &&
               existingFolder.SchoolYear.EndYear == request.SchoolYearEnd &&
               existingFolder.Semester == request.Semester &&
               existingFolder.CourseId == request.CourseId &&
               existingFolder.ParentFolderId == parentFolderId;
    }

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }
    }

    private static void EnsureSemesterIsValid(byte semester)
    {
        if (semester is < 1 or > 3)
        {
            throw new ArgumentException("Semester must be between 1 and 3.");
        }
    }

    private static void EnsureSchoolYearIsValid(int schoolYearStart, int schoolYearEnd)
    {
        if (schoolYearEnd != schoolYearStart + 1)
        {
            throw new ArgumentException("School year must span exactly one academic year.");
        }
    }

    private long? DecodeOptionalSqid(string? sqid, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(sqid))
        {
            return null;
        }

        if (!_sqidService.TryDecode(sqid.Trim(), out long decodedId))
        {
            throw new ArgumentException($"{parameterName} is invalid.");
        }

        return decodedId;
    }

    private FolderContentItemResponse ToContentItemResponse(Folder folder) => new()
    {
        Sqid = _sqidService.Encode(folder.FolderId),
        Name = folder.Name,
        CreatedAt = folder.CreatedAt,
        UpdatedAt = folder.UpdatedAt
    };

    private FolderContentItemResponse ToContentItemResponse(Document document) => new()
    {
        Sqid = _sqidService.Encode(document.DocumentId),
        Name = document.DocumentName,
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private FolderContentItemResponse ToContentItemResponse(Note note) => new()
    {
        Sqid = _sqidService.Encode(note.NoteId),
        Name = note.Name,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt
    };
}
