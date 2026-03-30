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
using EducAIte.Domain.ValueObjects;
using System.Text;

/// <summary>
/// Application service for StudyLoad operations.
/// Orchestrates business logic, validation, and domain mapping.
/// </summary>
public class StudyLoadService(
    IStudyLoadRepository studyLoadRepository,
    IStudentRepository studentRepository,
    IFileMetadataRepository fileMetadataRepository,
    ICourseRepository courseRepository,
    IFolderRepository folderRepository,
    IStudentCourseRepository studentCourseRepository,
    ILogger<StudyLoadService> logger,
    IAWSService awsService,
    ISqidService sqidService,
    IUnitOfWork unitOfWork
    ) : IStudyLoadService
{
    private readonly IStudyLoadRepository _studyLoadRepository = studyLoadRepository;
    private readonly IStudentRepository _studentRepository = studentRepository;
    private readonly IFileMetadataRepository _fileMetadataRepository = fileMetadataRepository;
    private readonly ICourseRepository _courseRepository = courseRepository;
    private readonly IFolderRepository _folderRepository = folderRepository;
    private readonly IStudentCourseRepository _studentCourseRepository = studentCourseRepository;
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

    public async Task<StudyLoadResponse> ApplyParsedCoursesAsync(
        string studyLoadSqid,
        ApplyParsedStudyLoadCoursesRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.", nameof(studentId));
        }

        if (!_sqidService.TryDecode(studyLoadSqid, out long studyLoadId))
        {
            throw new ArgumentException("Invalid study load sqid.", nameof(studyLoadSqid));
        }

        List<Course> normalizedCourses = request.Courses
            .Where(course => !string.IsNullOrWhiteSpace(course.EDPCode))
            .GroupBy(course => course.EDPCode.Trim(), StringComparer.Ordinal)
            .Select(group =>
            {
                CreateBulkCourseItemRequest item = group.First();
                return new Course
                {
                    EDPCode = item.EDPCode.Trim(),
                    CourseName = item.CourseName?.Trim() ?? string.Empty,
                    Units = item.Units
                };
            })
            .ToList();

        if (normalizedCourses.Count == 0)
        {
            throw new ArgumentException("At least one parsed course row is required.", nameof(request));
        }

        StudyLoad studyLoad = await _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken)
            ?? throw new KeyNotFoundException("Study load not found.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            IReadOnlySet<string> existingEdpCodes = await _courseRepository.GetExistingEdpCodesAsync(
                normalizedCourses.Select(course => course.EDPCode).ToArray(),
                cancellationToken);

            List<Course> missingCourses = normalizedCourses
                .Where(course => !existingEdpCodes.Contains(course.EDPCode))
                .ToList();

            if (missingCourses.Count > 0)
            {
                await _courseRepository.InsertMissingCoursesAsync(missingCourses, cancellationToken);
            }

            IReadOnlyList<Course> persistedCourses = await _courseRepository.GetByEdpCodesAsync(
                normalizedCourses.Select(course => course.EDPCode).ToArray(),
                cancellationToken);

            Dictionary<long, StudentCourse> existingEnrollments = (await _studentCourseRepository.GetByStudyLoadAndCourseIdsAsync(
                    studyLoadId,
                    persistedCourses.Select(course => course.CourseId).ToArray(),
                    includeDeleted: true,
                    cancellationToken: cancellationToken))
                .ToDictionary(studentCourse => studentCourse.CourseId);

            HashSet<long> attachedCourseIds = studyLoad.Courses
                .Select(course => course.CourseId)
                .ToHashSet();

            foreach (Course course in persistedCourses)
            {
                if (!attachedCourseIds.Contains(course.CourseId))
                {
                    studyLoad.Courses.Add(course);
                    attachedCourseIds.Add(course.CourseId);
                }

                if (existingEnrollments.TryGetValue(course.CourseId, out StudentCourse? existingEnrollment))
                {
                    if (existingEnrollment.IsDeleted)
                    {
                        existingEnrollment.Restore();
                        await _studentCourseRepository.UpdateAsync(existingEnrollment, cancellationToken);
                    }

                    continue;
                }

                StudentCourse studentCourse = new(course.CourseId, studyLoadId);
                await _studentCourseRepository.AddAsync(studentCourse, cancellationToken);
            }

            IReadOnlyList<StudentCourse> enrolledStudentCourses = await _studentCourseRepository.GetAllByStudyLoadIdAndStudentIdAsync(
                studyLoadId,
                studentId,
                cancellationToken);

            int createdFolderCount = await CreateMissingCourseFoldersAsync(
                enrolledStudentCourses,
                studentId,
                studyLoad.SchoolYearStart,
                studyLoad.SchoolYearEnd,
                (byte)studyLoad.Semester,
                cancellationToken);

            await _studyLoadRepository.UpdateStudyLoadAsync(studyLoad, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            StudyLoad refreshed = await _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken)
                ?? throw new InvalidOperationException("Study load was updated but could not be reloaded.");

            _logger.LogInformation(
                "Applied {CourseCount} parsed courses and created {FolderCount} folders for study load {StudyLoadId} for student {StudentId}",
                persistedCourses.Count,
                createdFolderCount,
                studyLoadId,
                studentId);

            return refreshed.ToResponse(_sqidService);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<int> CreateMissingCourseFoldersAsync(
        IReadOnlyList<StudentCourse> studentCourses,
        long studentId,
        int schoolYearStart,
        int schoolYearEnd,
        byte semester,
        CancellationToken cancellationToken)
    {
        if (studentCourses.Count == 0)
        {
            return 0;
        }

        List<StudentCourse> distinctStudentCourses = studentCourses
            .Where(studentCourse => studentCourse.StudentCourseId > 0)
            .GroupBy(studentCourse => studentCourse.StudentCourseId)
            .Select(group => group.First())
            .ToList();

        if (distinctStudentCourses.Count == 0)
        {
            return 0;
        }

        IReadOnlySet<long> existingStudentCourseIds = await _folderRepository.GetExistingStudentCourseIdsAsync(
            studentId,
            distinctStudentCourses.Select(studentCourse => studentCourse.StudentCourseId).ToArray(),
            cancellationToken);

        HashSet<string> reservedFolderKeys = new(
            await _folderRepository.GetAllFolderKeysAsync(studentId, cancellationToken),
            StringComparer.Ordinal);

        List<Folder> foldersToCreate = [];

        foreach (StudentCourse studentCourse in distinctStudentCourses)
        {
            if (existingStudentCourseIds.Contains(studentCourse.StudentCourseId))
            {
                continue;
            }

            string folderName = ResolveFolderName(studentCourse.Course);
            string folderKey = BuildUniqueFolderKey(folderName, reservedFolderKeys);

            foldersToCreate.Add(new Folder(
                studentId,
                new SchoolYear(schoolYearStart, schoolYearEnd),
                semester,
                folderKey,
                folderName,
                studentCourse.StudentCourseId));
        }

        if (foldersToCreate.Count == 0)
        {
            return 0;
        }

        await _folderRepository.AddRangeAsync(foldersToCreate, cancellationToken);
        return foldersToCreate.Count;
    }

    public Task<StudyLoadResponse?> GetByIdAndStudentIdAsync(string studentSqid, string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        _sqidService.TryDecode(studentSqid, out var studentId);
        _sqidService.TryDecode(studyLoadSqid, out var studyLoadId);

        return _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken)
            .ContinueWith(task => task.Result?.ToResponse(_sqidService), cancellationToken);
    }

    public async Task<StudyLoadResponse?> GetByIdAndStudentIdAsync(long studentId, string studyLoadSqid, CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.", nameof(studentId));
        }

        if (!_sqidService.TryDecode(studyLoadSqid, out var studyLoadId))
        {
            throw new ArgumentException("Invalid study load sqid.", nameof(studyLoadSqid));
        }

        StudyLoad? studyLoad = await _studyLoadRepository.GetByIdAndStudentIdAsync(studentId, studyLoadId, cancellationToken);
        return studyLoad?.ToResponse(_sqidService);
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

    private static string ResolveFolderName(Course course)
    {
        string resolvedName = string.IsNullOrWhiteSpace(course.CourseName)
            ? course.EDPCode
            : course.CourseName;

        return resolvedName.Trim();
    }

    private static string BuildUniqueFolderKey(string source, ISet<string> reservedFolderKeys)
    {
        string baseKey = SlugifyFolderKey(source);
        string candidate = baseKey;
        int suffix = 2;

        while (!reservedFolderKeys.Add(candidate))
        {
            candidate = AppendSuffix(baseKey, suffix);
            suffix++;
        }

        return candidate;
    }

    private static string SlugifyFolderKey(string value)
    {
        StringBuilder builder = new();
        bool previousDash = false;

        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                previousDash = false;
                continue;
            }

            if (builder.Length == 0 || previousDash)
            {
                continue;
            }

            builder.Append('-');
            previousDash = true;
        }

        string normalized = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "course";
        }

        return normalized.Length <= 100
            ? normalized
            : normalized[..100].TrimEnd('-');
    }

    private static string AppendSuffix(string baseKey, int suffix)
    {
        string suffixText = $"-{suffix}";
        int maxBaseLength = Math.Max(1, 100 - suffixText.Length);
        string truncatedBase = baseKey.Length > maxBaseLength
            ? baseKey[..maxBaseLength].TrimEnd('-')
            : baseKey;

        if (string.IsNullOrWhiteSpace(truncatedBase))
        {
            truncatedBase = "course";
        }

        return $"{truncatedBase}{suffixText}";
    }
}
