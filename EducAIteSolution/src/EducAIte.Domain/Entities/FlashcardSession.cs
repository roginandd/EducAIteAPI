namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class FlashcardSession
{
    public long FlashcardSessionId { get; private set; }
    public long StudentId { get; private set; }
    public long? StudentCourseId { get; private set; }
    public FlashcardSessionScopeType ScopeType { get; private set; }
    public FlashcardSessionStatus Status { get; private set; }
    public int CurrentItemIndex { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? RestartedFromSessionId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Student Student { get; private set; } = null!;
    public StudentCourse? StudentCourse { get; private set; }
    public ICollection<FlashcardSessionItem> Items { get; private set; } = new HashSet<FlashcardSessionItem>();

    private FlashcardSession()
    {
    }

    public FlashcardSession(long studentId, long? studentCourseId, FlashcardSessionScopeType scopeType)
    {
        StudentId = ValidatePositiveId(studentId, nameof(studentId));
        if (scopeType == FlashcardSessionScopeType.Course)
        {
            StudentCourseId = ValidatePositiveId(studentCourseId ?? 0, nameof(studentCourseId));
        }
        else
        {
            StudentCourseId = studentCourseId;
        }

        ScopeType = scopeType;
        Status = FlashcardSessionStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        LastActiveAt = StartedAt;
        UpdatedAt = StartedAt;
    }

    public void Start(DateTime startedAt)
    {
        DateTime normalized = startedAt.ToUniversalTime();
        Status = FlashcardSessionStatus.InProgress;
        StartedAt = normalized;
        LastActiveAt = normalized;
        UpdatedAt = normalized;
    }

    public void Resume(DateTime lastActiveAt)
    {
        EnsureNotDeleted();
        LastActiveAt = lastActiveAt.ToUniversalTime();
        UpdatedAt = LastActiveAt;
    }

    public void AdvanceTo(int index)
    {
        EnsureNotDeleted();
        CurrentItemIndex = Math.Max(0, index);
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = LastActiveAt;
    }

    public void MarkCompleted(DateTime completedAt)
    {
        EnsureNotDeleted();
        Status = FlashcardSessionStatus.Completed;
        CompletedAt = completedAt.ToUniversalTime();
        LastActiveAt = CompletedAt.Value;
        UpdatedAt = CompletedAt.Value;
    }

    public void MarkAbandoned(DateTime at)
    {
        EnsureNotDeleted();
        Status = FlashcardSessionStatus.Abandoned;
        LastActiveAt = at.ToUniversalTime();
        UpdatedAt = LastActiveAt;
    }

    public FlashcardSession RestartAsNew(DateTime now)
    {
        EnsureNotDeleted();
        Status = FlashcardSessionStatus.Restarted;
        LastActiveAt = now.ToUniversalTime();
        UpdatedAt = LastActiveAt;

        FlashcardSession restarted = new(StudentId, StudentCourseId, ScopeType);
        restarted.RestartedFromSessionId = FlashcardSessionId;
        restarted.Start(now);
        return restarted;
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

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify a deleted flashcard session.");
        }
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }
}
