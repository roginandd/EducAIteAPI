using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Extensions.MappingExtensions;
using EducAIte.Application.Interfaces;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using EducAIte.Domain.Enum;
using EducAIte.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducAIte.Application.Services.Implementation;

public sealed class FlashcardWorkspaceService : IFlashcardWorkspaceService
{
    private const string HiddenWorkspaceNoteName = "__flashcards_workspace__";
    private const string HiddenWorkspaceNoteContent = "# Flashcards Workspace";

    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IStudyLoadRepository _studyLoadRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IStudentFlashcardRepository _studentFlashcardRepository;
    private readonly ISqidService _sqidService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FlashcardWorkspaceService> _logger;

    public FlashcardWorkspaceService(
        IStudentCourseRepository studentCourseRepository,
        IStudyLoadRepository studyLoadRepository,
        IFolderRepository folderRepository,
        IDocumentRepository documentRepository,
        INoteRepository noteRepository,
        IFlashcardRepository flashcardRepository,
        IStudentFlashcardRepository studentFlashcardRepository,
        ISqidService sqidService,
        IUnitOfWork unitOfWork,
        ILogger<FlashcardWorkspaceService> logger)
    {
        _studentCourseRepository = studentCourseRepository;
        _studyLoadRepository = studyLoadRepository;
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
        _noteRepository = noteRepository;
        _flashcardRepository = flashcardRepository;
        _studentFlashcardRepository = studentFlashcardRepository;
        _sqidService = sqidService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FlashcardWorkspaceLatestResponse> GetLatestWorkspaceAsync(
        long studentId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StudyLoad> studyLoads = (await _studyLoadRepository.GetAllStudyLoadsByStudentIdAsync(
            studentId,
            cancellationToken)).ToList();

        StudyLoad? latestStudyLoad = studyLoads
            .OrderByDescending(studyLoad => studyLoad.SchoolYearStart)
            .ThenByDescending(studyLoad => studyLoad.SchoolYearEnd)
            .ThenByDescending(studyLoad => (int)studyLoad.Semester)
            .ThenByDescending(studyLoad => studyLoad.CreatedAt)
            .FirstOrDefault();

        if (latestStudyLoad is null)
        {
            return new FlashcardWorkspaceLatestResponse
            {
                LatestGroupLabel = string.Empty,
                SchoolYearStart = 0,
                SchoolYearEnd = 0,
                Semester = 0,
                Decks = [],
            };
        }

        IReadOnlyList<StudentCourse> studentCourses = await _studentCourseRepository.GetAllByStudyLoadIdAndStudentIdAsync(
            latestStudyLoad.StudyLoadId,
            studentId,
            cancellationToken);

        IReadOnlyList<Folder> allFolders = await _folderRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        Dictionary<long, Folder> rootFoldersByCourseId = studentCourses
            .Select(studentCourse => new
            {
                StudentCourseId = studentCourse.StudentCourseId,
                Folder = ResolveRootFolder(studentCourse, allFolders),
            })
            .Where(item => item.Folder is not null)
            .ToDictionary(item => item.StudentCourseId, item => item.Folder!);

        IReadOnlyList<Document> allDocuments = await _documentRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        Dictionary<long, long> courseIdByFolderId = rootFoldersByCourseId.Values
            .ToDictionary(folder => folder.FolderId, folder => folder.StudentCourseId);

        Dictionary<long, int> documentCountByCourseId = allDocuments
            .Where(document => courseIdByFolderId.ContainsKey(document.FolderId))
            .GroupBy(document => courseIdByFolderId[document.FolderId])
            .ToDictionary(group => group.Key, group => group.Count());

        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        Dictionary<long, int> flashcardCountByCourseId = flashcards
            .Where(flashcard => courseIdByFolderId.ContainsKey(flashcard.Note.Document.FolderId))
            .GroupBy(flashcard => courseIdByFolderId[flashcard.Note.Document.FolderId])
            .ToDictionary(group => group.Key, group => group.Count());

        IReadOnlyList<FlashcardDeckResponse> decks = studentCourses
            .OrderBy(studentCourse => studentCourse.Course.EDPCode)
            .ThenBy(studentCourse => studentCourse.Course.CourseName)
            .Select(studentCourse => new FlashcardDeckResponse
            {
                StudentCourseSqid = _sqidService.Encode(studentCourse.StudentCourseId),
                DeckName = studentCourse.Course.CourseName,
                EDPCode = studentCourse.Course.EDPCode,
                DocumentCount = documentCountByCourseId.GetValueOrDefault(studentCourse.StudentCourseId),
                FlashcardCount = flashcardCountByCourseId.GetValueOrDefault(studentCourse.StudentCourseId),
            })
            .ToList();

        return new FlashcardWorkspaceLatestResponse
        {
            LatestGroupLabel = BuildLatestGroupLabel(latestStudyLoad.Semester, latestStudyLoad.SchoolYearStart, latestStudyLoad.SchoolYearEnd),
            SchoolYearStart = latestStudyLoad.SchoolYearStart,
            SchoolYearEnd = latestStudyLoad.SchoolYearEnd,
            Semester = (int)latestStudyLoad.Semester,
            Decks = decks,
        };
    }

    public async Task<IReadOnlyList<FlashcardDocumentResponse>> GetDocumentsAsync(
        string studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        StudentCourse studentCourse = await GetRequiredStudentCourseAsync(studentCourseSqid, studentId, cancellationToken);
        Folder rootFolder = await GetRequiredRootFolderAsync(studentCourse, studentId, cancellationToken);

        IReadOnlyList<Document> documents = await _documentRepository.GetAllByFolderIdAndStudentIdAsync(
            rootFolder.FolderId,
            studentId,
            cancellationToken);

        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        Dictionary<long, int> flashcardCountsByDocumentId = flashcards
            .Where(flashcard => flashcard.Note.Document.FolderId == rootFolder.FolderId)
            .GroupBy(flashcard => flashcard.Note.DocumentId)
            .ToDictionary(group => group.Key, group => group.Count());

        return documents
            .Select(document => new FlashcardDocumentResponse
            {
                Sqid = _sqidService.Encode(document.DocumentId),
                StudentCourseSqid = studentCourseSqid,
                Name = document.DocumentName,
                FlashcardCount = flashcardCountsByDocumentId.GetValueOrDefault(document.DocumentId),
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<FlashcardResponse>> GetFlashcardsAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        Document document = await GetRequiredDocumentAsync(documentSqid, studentId, cancellationToken);
        IReadOnlyList<Flashcard> flashcards = await _flashcardRepository.GetAllByDocumentIdAndStudentIdAsync(
            document.DocumentId,
            studentId,
            cancellationToken);

        return flashcards.Select(flashcard => flashcard.ToResponse(_sqidService)).ToList();
    }

    public async Task<FlashcardResponse> CreateFlashcardAsync(
        string documentSqid,
        CreateWorkspaceFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        Document document = await GetRequiredTrackedDocumentAsync(documentSqid, studentId, cancellationToken);
        Note workspaceNote = await EnsureWorkspaceNoteAsync(document, cancellationToken);

        Flashcard flashcard = new(
            request.Question,
            request.Answer,
            workspaceNote.NoteId,
            request.ConceptExplanation,
            request.AnsweringGuidance,
            request.AcceptedAnswerAliases);

        workspaceNote.AddFlashcard(flashcard);
        Flashcard created = await _flashcardRepository.AddAsync(flashcard, cancellationToken);

        _logger.LogInformation(
            "Created workspace flashcard {FlashcardId} under document {DocumentId}",
            created.FlashcardId,
            document.DocumentId);

        return created.ToResponse(_sqidService);
    }

    public async Task<bool> UpdateFlashcardAsync(
        string documentSqid,
        string flashcardSqid,
        UpdateWorkspaceFlashcardRequest request,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        Document document = await GetRequiredDocumentAsync(documentSqid, studentId, cancellationToken);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        Flashcard? existing = await _flashcardRepository.GetTrackedByIdAndStudentIdAsync(
            flashcardId,
            studentId,
            cancellationToken);

        if (existing is null || existing.Note.DocumentId != document.DocumentId)
        {
            return false;
        }

        existing.UpdateContent(
            request.Question,
            request.Answer,
            request.ConceptExplanation,
            request.AnsweringGuidance,
            request.AcceptedAnswerAliases);

        await _flashcardRepository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteFlashcardAsync(
        string documentSqid,
        string flashcardSqid,
        long studentId,
        CancellationToken cancellationToken = default)
    {
        Document document = await GetRequiredDocumentAsync(documentSqid, studentId, cancellationToken);
        long flashcardId = DecodeRequiredSqid(flashcardSqid, nameof(flashcardSqid));

        Flashcard? existing = await _flashcardRepository.GetTrackedByIdAndStudentIdAsync(
            flashcardId,
            studentId,
            cancellationToken);

        if (existing is null || existing.Note.DocumentId != document.DocumentId)
        {
            return false;
        }

        IReadOnlyList<StudentFlashcard> progressEntries = await _studentFlashcardRepository.GetTrackedByFlashcardIdAsync(
            flashcardId,
            cancellationToken);

        existing.MarkDeletedWithProgress(progressEntries);
        await _flashcardRepository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    private async Task<StudentCourse> GetRequiredStudentCourseAsync(
        string studentCourseSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        long studentCourseId = DecodeRequiredSqid(studentCourseSqid, nameof(studentCourseSqid));

        StudentCourse? studentCourse = await _studentCourseRepository.GetByIdAndStudentIdAsync(
            studentCourseId,
            studentId,
            cancellationToken);

        return studentCourse ?? throw new KeyNotFoundException("Student course not found.");
    }

    private async Task<Folder> GetRequiredRootFolderAsync(
        StudentCourse studentCourse,
        long studentId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Folder> folders = await _folderRepository.GetAllByStudentIdAsync(studentId, cancellationToken);
        Folder? rootFolder = ResolveRootFolder(studentCourse, folders);

        return rootFolder ?? throw new KeyNotFoundException("Student course root folder not found.");
    }

    private async Task<Document> GetRequiredDocumentAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        long documentId = DecodeRequiredSqid(documentSqid, nameof(documentSqid));
        Document? document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null || document.Folder.StudentId != studentId)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        return document;
    }

    private async Task<Document> GetRequiredTrackedDocumentAsync(
        string documentSqid,
        long studentId,
        CancellationToken cancellationToken)
    {
        long documentId = DecodeRequiredSqid(documentSqid, nameof(documentSqid));
        Document? document = await _documentRepository.GetTrackedByIdAsync(documentId, cancellationToken);

        if (document is null || document.Folder.StudentId != studentId)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        return document;
    }

    private async Task<Note> EnsureWorkspaceNoteAsync(Document document, CancellationToken cancellationToken)
    {
        Note? existing = document.Notes.FirstOrDefault(note =>
            !note.IsDeleted &&
            string.Equals(note.Name, HiddenWorkspaceNoteName, StringComparison.Ordinal));

        if (existing is not null)
        {
            return existing;
        }

        Note? lastNote = await _noteRepository.GetLastByDocumentIdAsync(document.DocumentId, cancellationToken);
        decimal sequenceNumber = lastNote is null ? 1024m : lastNote.SequenceNumber + 1024m;

        Note note = new(HiddenWorkspaceNoteName, HiddenWorkspaceNoteContent, document.DocumentId, sequenceNumber);
        document.AddNote(note);
        await _noteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return note;
    }

    private static Folder? ResolveRootFolder(StudentCourse studentCourse, IReadOnlyList<Folder> folders)
    {
        Folder? exact = folders
            .Where(folder =>
                folder.ParentFolderId is null &&
                folder.StudentCourseId == studentCourse.StudentCourseId)
            .OrderBy(folder => folder.CreatedAt)
            .FirstOrDefault();

        if (exact is not null)
        {
            return exact;
        }

        return folders
            .Where(folder =>
                folder.ParentFolderId is null &&
                folder.StudentCourseId == studentCourse.CourseId &&
                folder.SchoolYear.StartYear == studentCourse.StudyLoad.SchoolYearStart &&
                folder.SchoolYear.EndYear == studentCourse.StudyLoad.SchoolYearEnd &&
                folder.Semester == (byte)studentCourse.StudyLoad.Semester)
            .OrderBy(folder => folder.CreatedAt)
            .FirstOrDefault();
    }

    private long DecodeRequiredSqid(string sqid, string fieldName)
    {
        if (!_sqidService.TryDecode(sqid, out long id))
        {
            throw new ArgumentException("Invalid sqid.", fieldName);
        }

        return id;
    }

    private static string BuildLatestGroupLabel(Semester semester, int schoolYearStart, int schoolYearEnd)
    {
        return $"{GetSemesterLabel((int)semester)} | SY {schoolYearStart}-{schoolYearEnd}";
    }

    private static string GetSemesterLabel(int semester)
    {
        return semester switch
        {
            1 => "1st Semester",
            2 => "2nd Semester",
            3 => "3rd Semester",
            _ => $"Semester {semester}",
        };
    }
}
