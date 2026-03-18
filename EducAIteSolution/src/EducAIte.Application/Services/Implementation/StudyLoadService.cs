namespace EducAIte.Application.Services.Implementation;

using EducAIte.Application.DTOs;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Interfaces;
using EducAIte.Domain.Entities;

/// <summary>
/// Application service for StudyLoad operations.
/// Orchestrates business logic, validation, and domain mapping.
/// </summary>
public class StudyLoadService(
    IStudyLoadRepository studyLoadRepository,
    IStudentRepository studentRepository,
    IFileMetadataRepository fileMetadataRepository,
    ILogger<StudyLoadService> logger,
    IAWSService awsService,
    ISqidService sqidService,
    IUnitOfWork unitOfWork
    ) : IStudyLoadService
{
    private readonly IStudyLoadRepository _studyLoadRepository = studyLoadRepository;
    private readonly IStudentRepository _studentRepository = studentRepository;
    private readonly IFileMetadataRepository _fileMetadataRepository = fileMetadataRepository;
    private readonly ILogger<StudyLoadService> _logger = logger;
    private readonly IAWSService _awsService = awsService;
    private readonly ISqidService _sqidService = sqidService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<StudyLoadDto> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default)
    {

        StudyLoad studyLoad = studyLoadCreateDto.ToEntity(_sqidService.TryDecode(studyLoadCreateDto.StudentSqid, out var studentId) ? studentId : throw new ArgumentException("Invalid Student Sqid"));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        string path = await _awsService.UploadStudyLoad(studyLoadCreateDto, cancellationToken);

        FileMetadata fileMetadata = new()
        {
          FileName = studyLoadCreateDto.StudyLoadDocument.FileName,
          FileExtension = Path.GetExtension(studyLoadCreateDto.StudyLoadDocument.FileName),
          ContentType = studyLoadCreateDto.StudyLoadDocument.ContentType,
          StorageKey = path,
          FileSizeInBytes = studyLoadCreateDto.StudyLoadDocument.Length,
          UploadedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow,
          StudentId = studentId  
        };

        FileMetadata addedFileMetadata = await _fileMetadataRepository.AddFileMetadataAsync(fileMetadata, cancellationToken);

        studyLoad.FileMetadata = addedFileMetadata;
        studyLoad.FileMetadataId = addedFileMetadata.FileMetaDataId;

        await _studyLoadRepository.AddStudyLoadAsync(studyLoad, cancellationToken);

        // Buhatonon: Bulk enroll and add course associations here

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return studyLoad.ToDto(_sqidService);

    }

    public Task<bool> DeleteStudyLoadAsync(string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<StudyLoadDto>> GetAllStudyLoadsByStudentIdAsync(string studentSqid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StudyLoadDto?> GetStudyLoadByIdAsync(string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StudyLoadDto> UpdateStudyLoadAsync(StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
