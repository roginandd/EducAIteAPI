namespace EducAIte.Application.Services.Implementation;

using EducAIte.Application.DTOs;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;

/// <summary>
/// Application service for StudyLoad operations.
/// Orchestrates business logic, validation, and domain mapping.
/// </summary>
public class StudyLoadService(
    IStudyLoadRepository studyLoadRepository,
    IStudentRepository studentRepository,
    ILogger<StudyLoadService> logger,
    IAWSService awsService,
    ISqidService sqidService) : IStudyLoadService
{
    private readonly IStudyLoadRepository _studyLoadRepository = studyLoadRepository;
    private readonly IStudentRepository _studentRepository = studentRepository;
    private readonly ILogger<StudyLoadService> _logger = logger;
    private readonly IAWSService _awsService = awsService;
    private readonly ISqidService _sqidService = sqidService;

    /// <summary>
    /// Retrieves a study load by student ID.
    /// </summary>
    public async Task<StudyLoadDto?> GetStudyLoadByStudentIdAsync(long studentId, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByStudentIdAsync(studentId);
        if (student is null)
        {
            _logger.LogWarning("Attempted to retrieve study load for non-existent student ID {StudentId}.", studentId);
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        var studyLoad = await _studyLoadRepository.GetByStudentIdAsync(studentId, cancellationToken);
        if (studyLoad is null)
        {
            _logger.LogInformation("Study load for student ID {StudentId} not found.", studentId);
            return null;
        }

        _logger.LogInformation("Retrieved study load for student ID {StudentId}.", studentId);
        return studyLoad.ToDto(_sqidService);
    }

    /// <summary>
    /// Creates a new study load for a student.
    /// Validates that the student exists before creating the study load.
    /// </summary>
    public async Task<StudyLoadDto> AddStudyLoadAsync(StudyLoadCreateRequest studyLoadCreateDto, CancellationToken cancellationToken = default)
    {
        if (!_sqidService.TryDecode(studyLoadCreateDto.StudentSqid, out long studentId))
        {
            throw new ArgumentException("StudentSqid is invalid.", nameof(studyLoadCreateDto.StudentSqid));
        }

        // Validate student exists
        var student = await _studentRepository.GetByStudentIdAsync(studentId);
        if (student is null)
        {
            _logger.LogWarning("Attempted to create study load for non-existent student ID {StudentId}.", studentId);
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        // Check if student already has a study load
        var existingStudyLoad = await _studyLoadRepository.GetByStudentIdAsync(studentId, cancellationToken);
        if (existingStudyLoad is not null)
        {
            throw new InvalidOperationException($"Student {studentId} already has a study load.");
        }

        await _awsService.UploadStudyLoad(studyLoadCreateDto, cancellationToken);

        var newStudyLoad = studyLoadCreateDto.ToEntity(studentId);
        var createdStudyLoad = await _studyLoadRepository.AddStudyLoadAsync(newStudyLoad, cancellationToken);
        _logger.LogInformation("Study load added for student ID {StudentId}.", createdStudyLoad.StudentId);
        return createdStudyLoad.ToDto(_sqidService);
    }

    /// <summary>
    /// Updates an existing study load.
    /// </summary>
    public async Task<StudyLoadDto> UpdateStudyLoadAsync(long id, StudyLoadUpdateRequest studyLoadUpdateDto, CancellationToken cancellationToken = default)
    {
        var existingStudyLoad = await _studyLoadRepository.GetByStudentIdAsync(id, cancellationToken);
        if (existingStudyLoad is null)
        {
            _logger.LogWarning("Study load with student ID {StudentId} not found for update.", id);
            throw new KeyNotFoundException($"Study load for student ID {id} not found.");
        }

        var updatedStudyLoad = studyLoadUpdateDto.ToEntity(existingStudyLoad);
        var result = await _studyLoadRepository.UpdateStudyLoadAsync(updatedStudyLoad, cancellationToken);
        _logger.LogInformation("Study load with ID {StudyLoadId} updated.", result.StudyLoadId);
        return result.ToDto(_sqidService);
    }

    /// <summary>
    /// Deletes a study load by ID.
    /// </summary>
    public async Task<bool> DeleteStudyLoadAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _studyLoadRepository.DeleteStudyLoadAsync(id, cancellationToken);
        if (deleted)
        {
            _logger.LogInformation("Study load with ID {StudyLoadId} deleted.", id);
        }
        else
        {
            _logger.LogWarning("Study load with ID {StudyLoadId} not found for deletion.", id);
        }
        return deleted;
    }
}
