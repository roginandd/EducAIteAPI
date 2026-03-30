using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IFileMetadataRepository _fileMetadataRepository;
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly IAWSService _awsService;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IStudentRepository studentRepository,
        IFolderRepository folderRepository,
        IFileMetadataRepository fileMetadataRepository,
        IResourceOwnershipService resourceOwnershipService,
        IAWSService awsService,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _studentRepository = studentRepository;
        _folderRepository = folderRepository;
        _fileMetadataRepository = fileMetadataRepository;
        _resourceOwnershipService = resourceOwnershipService;
        _awsService = awsService;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DocumentResponse?> GetDocumentByIdAsync(
        string sqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long documentId))
            return null;

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        return document is null ? null : ToResponse(document);
    }

    public async Task<SignedUrlResponse?> GetSignedUrlAsync(
        string sqid,
        long studentId,
        int expiresInMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (expiresInMinutes <= 0)
        {
            throw new ArgumentException("expiresInMinutes must be greater than zero.", nameof(expiresInMinutes));
        }

        if (!_sqidService.TryDecode(sqid, out long documentId))
        {
            return null;
        }

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        string url = _awsService.GenerateSignedUrl(
            document.FileMetadata.StorageKey,
            TimeSpan.FromMinutes(expiresInMinutes),
            cancellationToken);

        return new SignedUrlResponse
        {
            Url = url
        };
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(
        long studentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);

        IReadOnlyList<Document> documents = await _documentRepository.GetAllByStudentIdAsync(studentId, cancellationToken);

        return documents.Select(ToResponse);
    }

    public async Task<UploadFolderDocumentResponse> UploadToFolderAsync(
        string folderSqid,
        UploadFolderDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        ArgumentNullException.ThrowIfNull(request);

        if (!_sqidService.TryDecode(folderSqid, out long folderId))
        {
            throw new ArgumentException("Folder sqid is invalid.", nameof(folderSqid));
        }

        if (request.File.Length == 0)
        {
            throw new ArgumentException("File is required.", nameof(request));
        }

        await _resourceOwnershipService.EnsureFolderOwnedByStudentAsync(folderId, studentId, cancellationToken);

        Folder? folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
        {
            throw new KeyNotFoundException($"Folder with ID {folderId} not found.");
        }

        string studentSqid = _sqidService.Encode(studentId);
        string extension = Path.GetExtension(request.File.FileName);
        string storageKey = $"users/{studentSqid}/folders/{folderSqid}/documents/{Guid.NewGuid()}{extension}";

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            string uploadedKey = await _awsService.UploadFileAsync(request.File, storageKey, cancellationToken);

            FileMetadata fileMetadata = new()
            {
                FileName = request.File.FileName,
                FileExtension = extension,
                ContentType = request.File.ContentType,
                StorageKey = uploadedKey,
                FileSizeInBytes = request.File.Length,
                UploadedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StudentId = studentId
            };

            FileMetadata createdFileMetadata = await _fileMetadataRepository.AddFileMetadataAsync(fileMetadata, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Document document = new(
                ResolveDocumentName(request),
                folderId,
                createdFileMetadata.FileMetaDataId);

            await _documentRepository.AddAsync(document, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            Document? createdDocument = await _documentRepository.GetByIdAsync(document.DocumentId, cancellationToken);
            if (createdDocument is null)
            {
                throw new InvalidOperationException("Document was created but could not be reloaded.");
            }

            _logger.LogInformation("Uploaded document {DocumentId} to folder {FolderId}", document.DocumentId, folderId);

            return new UploadFolderDocumentResponse
            {
                Document = ToResponse(createdDocument),
                FileMetadata = createdFileMetadata.ToResponse(_sqidService)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading document to folder {FolderSqid}", folderSqid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<DocumentResponse> CreateDocumentAsync(
        CreateDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);
        Document document = request.ToEntity();
        await ValidateOwnershipAsync(document.FolderId, document.FileMetadataId, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _documentRepository.AddAsync(document, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating document for student {StudentId}", studentId);
            throw;
        }

        Document? created = await _documentRepository.GetByIdAsync(document.DocumentId, cancellationToken);

        _logger.LogInformation("Created document {DocumentId}", document.DocumentId);

        if (created is null)
        {
            throw new InvalidOperationException("Document was created but could not be reloaded.");
        }

        return ToResponse(created);
    }

    public async Task<bool> UpdateDocumentAsync(
        string sqid,
        UpdateDocumentRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long documentId))
            return false;

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        Document? existingDocument = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (existingDocument is null)
            return false;

        (long folderId, long fileMetadataId) = ResolveUpdateReferences(request, existingDocument);

        if (IsUnchanged(existingDocument, request, folderId, fileMetadataId))
            return true;

        await ValidateOwnershipAsync(folderId, fileMetadataId, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            existingDocument.UpdateDetails(request.DocumentName, folderId, fileMetadataId);
            await _documentRepository.UpdateAsync(existingDocument, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating document {DocumentSqid}", sqid);
            throw;
        }
        _logger.LogInformation("Updated document {DocumentSqid}", sqid);

        return true;
    }

    public async Task<bool> DeleteDocumentAsync(
        string sqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureStudentIdIsValid(studentId);

        if (!_sqidService.TryDecode(sqid, out long documentId))
            return false;

        await _resourceOwnershipService.EnsureDocumentOwnedByStudentAsync(documentId, studentId, cancellationToken);
        Document? existingDocument = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (existingDocument is null)
            return false;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _documentRepository.DeleteAsync(documentId, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting document {DocumentSqid}", sqid);
            throw;
        }

        _logger.LogInformation("Deleted document {DocumentSqid}", sqid);

        return true;
    }

    private (long FolderId, long FileMetadataId) ResolveUpdateReferences(UpdateDocumentRequest request, Document existingDocument)
    {
        long folderId = request.FolderId ?? existingDocument.FolderId;
        long fileMetadataId = request.FileMetadataId ?? existingDocument.FileMetadataId;

        if (folderId <= 0 && !string.IsNullOrWhiteSpace(request.FolderSqid))
        {
            if (!_sqidService.TryDecode(request.FolderSqid, out folderId))
            {
                throw new ArgumentException("FolderSqid is invalid.", nameof(request));
            }
        }

        if (fileMetadataId <= 0 && !string.IsNullOrWhiteSpace(request.FileMetadataSqid))
        {
            if (!_sqidService.TryDecode(request.FileMetadataSqid, out fileMetadataId))
            {
                throw new ArgumentException("FileMetadataSqid is invalid.", nameof(request));
            }
        }

        if (folderId <= 0)
        {
            throw new ArgumentException("Folder reference is required.", nameof(request));
        }

        if (fileMetadataId <= 0)
        {
            throw new ArgumentException("File metadata reference is required.", nameof(request));
        }

        return (folderId, fileMetadataId);
    }

    private static bool IsUnchanged(Document existingDocument, UpdateDocumentRequest request, long folderId, long fileMetadataId)
    {
        return string.Equals(existingDocument.DocumentName, request.DocumentName.Trim(), StringComparison.Ordinal) &&
               existingDocument.FolderId == folderId &&
               existingDocument.FileMetadataId == fileMetadataId;
    }

    private async Task EnsureStudentExistsAsync(long studentId, CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            throw new ArgumentException("StudentId must be greater than zero.");

        Student? student = await _studentRepository.GetByStudentIdAsync(studentId, cancellationToken);
        if (student is null || student.IsDeleted)
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
    }

    private async Task ValidateOwnershipAsync(
        long folderId,
        long fileMetadataId,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        long? folderStudentId = await _documentRepository.GetFolderStudentIdAsync(folderId, cancellationToken);
        if (folderStudentId is null)
            throw new KeyNotFoundException($"Folder with ID {folderId} not found.");

        long? fileStudentId = await _documentRepository.GetFileMetadataStudentIdAsync(fileMetadataId, cancellationToken);
        if (fileStudentId is null)
            throw new KeyNotFoundException($"File metadata with ID {fileMetadataId} not found.");

        if (folderStudentId.Value != fileStudentId.Value)
            throw new InvalidOperationException("Folder and file metadata must belong to the same student.");

        if (folderStudentId.Value != studentId)
        {
            throw new UnauthorizedAccessException("Folder and file metadata must belong to the authenticated student.");
        }
    }

    private DocumentResponse ToResponse(Document document) => new()
    {
        Sqid = _sqidService.Encode(document.DocumentId),
        DocumentName = document.DocumentName,
        FolderSqid = _sqidService.Encode(document.FolderId),
        FileMetadataSqid = _sqidService.Encode(document.FileMetadataId),
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private static void EnsureStudentIdIsValid(long studentId)
    {
        if (studentId <= 0)
            throw new ArgumentException("StudentId must be greater than zero.");
    }

    private static string ResolveDocumentName(UploadFolderDocumentRequest request)
    {
        return string.IsNullOrWhiteSpace(request.DocumentName)
            ? request.File.FileName.Trim()
            : request.DocumentName.Trim();
    }
}
