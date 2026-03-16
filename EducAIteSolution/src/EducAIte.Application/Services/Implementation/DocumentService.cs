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
    private readonly IResourceOwnershipService _resourceOwnershipService;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IStudentRepository studentRepository,
        IResourceOwnershipService resourceOwnershipService,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _studentRepository = studentRepository;
        _resourceOwnershipService = resourceOwnershipService;
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

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(
        long studentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);

        IReadOnlyList<Document> documents = await _documentRepository.GetAllByStudentIdAsync(studentId, cancellationToken);

        return documents.Select(ToResponse);
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

        if (IsUnchanged(existingDocument, request))
            return true;

        await ValidateOwnershipAsync(request.FolderId, request.FileMetadataId, studentId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            request.ApplyToEntity(existingDocument);
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

    private static bool IsUnchanged(Document existingDocument, UpdateDocumentRequest request)
    {
        return string.Equals(existingDocument.DocumentName, request.DocumentName.Trim(), StringComparison.Ordinal) &&
               existingDocument.FolderId == request.FolderId &&
               existingDocument.FileMetadataId == request.FileMetadataId;
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
            throw new UnauthorizedAccessException("Folder and file metadata must belong to the authenticated student.");
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
}