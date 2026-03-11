using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISqidService _sqidService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IStudentRepository studentRepository,
        ISqidService sqidService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _studentRepository = studentRepository;
        _sqidService = sqidService;
        _logger = logger;
    }

    public async Task<DocumentResponse?> GetDocumentByIdAsync(string sqid, CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(sqid, out long documentId))
        {
            return null;
        }

        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        return document is null ? null : ToResponse(document);
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByStudentAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await EnsureStudentExistsAsync(studentId, cancellationToken);

        IReadOnlyList<Document> documents = await _documentRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        return documents.Select(ToResponse);
    }

    public async Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        Document document = request.ToEntity();
        await ValidateOwnershipAsync(document.FolderId, document.FileMetadataId, cancellationToken);

        Document createdDocument = await _documentRepository.AddAsync(document, cancellationToken);
        _logger.LogInformation("Created document {DocumentId}", createdDocument.DocumentId);

        return ToResponse(createdDocument);
    }

    public async Task<bool> UpdateDocumentAsync(string sqid, UpdateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(sqid, out long documentId))
        {
            return false;
        }

        Document? existingDocument = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (existingDocument is null)
        {
            return false;
        }

        if (IsUnchanged(existingDocument, request))
        {
            return true;
        }

        await ValidateOwnershipAsync(request.FolderId, request.FileMetadataId, cancellationToken);

        request.ApplyToEntity(existingDocument);
        await _documentRepository.UpdateAsync(existingDocument, cancellationToken);

        _logger.LogInformation("Updated document {DocumentSqid}", sqid);
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(string sqid, CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(sqid, out long documentId))
        {
            return false;
        }

        Document? existingDocument = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (existingDocument is null)
        {
            return false;
        }

        await _documentRepository.DeleteAsync(documentId, cancellationToken);
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
        {
            throw new ArgumentException("StudentId must be greater than zero.");
        }

        Student? student = await _studentRepository.GetByStudentIdAsync(studentId, cancellationToken);
        if (student is null || student.IsDeleted)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }
    }

    private async Task ValidateOwnershipAsync(long folderId, long fileMetadataId, CancellationToken cancellationToken = default)
    {
        long? folderStudentId = await _documentRepository.GetFolderStudentIdAsync(folderId, cancellationToken);
        if (folderStudentId is null)
        {
            throw new KeyNotFoundException($"Folder with ID {folderId} not found.");
        }

        long? fileStudentId = await _documentRepository.GetFileMetadataStudentIdAsync(fileMetadataId, cancellationToken);
        if (fileStudentId is null)
        {
            throw new KeyNotFoundException($"File metadata with ID {fileMetadataId} not found.");
        }

        if (folderStudentId.Value != fileStudentId.Value)
        {
            throw new InvalidOperationException("Folder and file metadata must belong to the same student.");
        }
    }

    private DocumentResponse ToResponse(Document document)
    {
        return new DocumentResponse
        {
            Sqid = _sqidService.Encode(document.DocumentId),
            DocumentName = document.DocumentName,
            FolderSqid = _sqidService.Encode(document.FolderId),
            FileMetadataSqid = _sqidService.Encode(document.FileMetadataId),
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
