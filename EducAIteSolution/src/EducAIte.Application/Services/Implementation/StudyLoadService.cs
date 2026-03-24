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
    public async Task<StudyLoadResponse> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default)
    {

        StudyLoad studyLoad = studyLoadCreateDto.ToEntity(_sqidService.TryDecode(studyLoadCreateDto.StudentSqid, out var studentId) ? studentId : throw new ArgumentException("Invalid Student Sqid"));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        string path = await _awsService.UploadStudyLoad(studyLoadCreateDto, cancellationToken);

        FileMetadata fileMetadata = studyLoadCreateDto.ToFileMetadataEntity(studentId, path);

        FileMetadata addedFileMetadata = await _fileMetadataRepository.AddFileMetadataAsync(fileMetadata, cancellationToken);

        studyLoad.FileMetadata = addedFileMetadata;
        studyLoad.FileMetadataId = addedFileMetadata.FileMetaDataId;

        await _studyLoadRepository.AddStudyLoadAsync(studyLoad, cancellationToken);

        // Buhatonon: Bulk enroll and add course associations here

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return studyLoad.ToResponse(_sqidService);

    }

    public Task<StudyLoadResponse?> GetByIdAndStudentIdAsync(string studentSqid, string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studentSqid, out var studentId);
        _sqidService.TryDecode(studyLoadSqid, out var studyLoadId);

        return _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken)
            .ContinueWith(task => task.Result?.ToResponse(_sqidService), cancellationToken);
    }

    public Task<bool> DeleteStudyLoadAsync(string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studyLoadSqid, out var studyLoadId);
        return _studyLoadRepository.DeleteStudyLoadAsync(studyLoadId, cancellationToken);
    }

    public async Task<IEnumerable<StudyLoadResponse>> GetAllStudyLoadsByStudentIdAsync(string studentSqid, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studentSqid, out var studentId);
        
        var studyLoads = await _studyLoadRepository.GetAllStudyLoadsByStudentIdAsync(studentId, cancellationToken);
        return studyLoads.Select(sl => sl.ToResponse(_sqidService));
    }

    public Task<StudyLoadResponse?> GetStudyLoadByIdAsync(string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studyLoadSqid, out var studyLoadId);
        return _studyLoadRepository.GetStudyLoadByIdAsync(studyLoadId, cancellationToken)
            .ContinueWith(task => task.Result?.ToResponse(_sqidService), cancellationToken);
    }

    public Task<StudyLoadResponse> UpdateStudyLoadAsync(StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studyLoadUpdateDto.StudyLoadSqid, out var studyLoadId);
        _sqidService.TryDecode(studyLoadUpdateDto.StudentSqid, out var studentId);

        return _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken)
            .ContinueWith(task =>
            {
                StudyLoad? existingStudyLoad = task.Result ?? throw new KeyNotFoundException("Study load not found.");
                // Apply updates from DTO to entity

                // Buhatonon: Handle course associations here

                return _studyLoadRepository.UpdateStudyLoadAsync(existingStudyLoad, cancellationToken)
                    .ContinueWith(updateTask => updateTask.Result.ToResponse(_sqidService), cancellationToken);
            }, cancellationToken).Unwrap();   
    }
}
