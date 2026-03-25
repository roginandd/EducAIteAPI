using EducAIte.Domain.ValueObjects;

namespace EducAIte.Domain.Entities;


public class Folder
{
    public long FolderId { get; set; }

    public long StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public SchoolYear SchoolYear { get; set; } = new(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 1);

    public byte Semester { get; set; }

    public string FolderKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public long? CourseId { get; set; } // Link to the actual Course entity
    public Course? Course { get; set; }

    public long? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }

    public ICollection<Folder> SubFolders { get; set; } = new HashSet<Folder>();
    public ICollection<Document> Documents { get; set; } = new HashSet<Document>();


    //Additional Properties
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private Folder()
    {
    }

    public Folder(
        long studentId,
        SchoolYear schoolYear,
        byte semester,
        string folderKey,
        string name,
        long? courseId = null,
        long? parentFolderId = null)
    {
        StudentId = ValidateStudentId(studentId);
        SchoolYear = ValidateSchoolYear(schoolYear);
        Semester = ValidateSemester(semester);
        FolderKey = NormalizeFolderKey(folderKey);
        Name = NormalizeName(name);
        CourseId = ValidateOptionalPositiveId(courseId, nameof(courseId));
        ParentFolderId = ValidateOptionalPositiveId(parentFolderId, nameof(parentFolderId));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        SchoolYear schoolYear,
        byte semester,
        string folderKey,
        string name,
        long? courseId,
        long? parentFolderId)
    {
        EnsureNotDeleted();

        SchoolYear = ValidateSchoolYear(schoolYear);
        Semester = ValidateSemester(semester);
        FolderKey = NormalizeFolderKey(folderKey);
        Name = NormalizeName(name);
        CourseId = ValidateOptionalPositiveId(courseId, nameof(courseId));
        ParentFolderId = ValidateOptionalPositiveId(parentFolderId, nameof(parentFolderId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeletedWithChildren(IEnumerable<Folder> subFolders, IEnumerable<Document> documents)
    {
        ArgumentNullException.ThrowIfNull(subFolders);
        ArgumentNullException.ThrowIfNull(documents);

        foreach (Document document in documents)
        {
            document.MarkDeletedWithChildren(document.Notes);
        }

        foreach (Folder subFolder in subFolders)
        {
            subFolder.MarkDeleted();
        }

        MarkDeleted();
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted folder.");
        }
    }

    private static long ValidateStudentId(long studentId)
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("StudentId must be greater than zero.", nameof(studentId));
        }

        return studentId;
    }

    private static SchoolYear ValidateSchoolYear(SchoolYear schoolYear)
    {
        ArgumentNullException.ThrowIfNull(schoolYear);

        if (!schoolYear.IsValid)
        {
            throw new ArgumentException("School year must span exactly one academic year.", nameof(schoolYear));
        }

        return schoolYear;
    }

    private static byte ValidateSemester(byte semester)
    {
        if (semester is < 1 or > 3)
        {
            throw new ArgumentException("Semester must be between 1 and 3.", nameof(semester));
        }

        return semester;
    }

    private static string NormalizeFolderKey(string folderKey)
    {
        if (string.IsNullOrWhiteSpace(folderKey))
        {
            throw new ArgumentException("Folder key is required.", nameof(folderKey));
        }

        string normalized = folderKey.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentException("Folder key cannot exceed 100 characters.", nameof(folderKey));
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Folder name is required.", nameof(name));
        }

        string normalized = name.Trim();
        if (normalized.Length > 200)
        {
            throw new ArgumentException("Folder name cannot exceed 200 characters.", nameof(name));
        }

        return normalized;
    }

    private static long? ValidateOptionalPositiveId(long? id, string parameterName)
    {
        if (id is <= 0)
        {
            throw new ArgumentException($"{parameterName} must be greater than zero when provided.", parameterName);
        }

        return id;
    }
}
