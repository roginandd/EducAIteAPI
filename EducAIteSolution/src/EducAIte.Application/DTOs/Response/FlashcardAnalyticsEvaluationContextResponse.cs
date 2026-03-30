namespace EducAIte.Application.DTOs.Response;

public record FlashcardAnalyticsEvaluationContextResponse
{
    public required string FlashcardSqid { get; init; } = string.Empty;

    public required string StudentCourseSqid { get; init; } = string.Empty;

    public required string Question { get; init; } = string.Empty;

    public required string ExpectedAnswer { get; init; } = string.Empty;

    public string ConceptExplanation { get; init; } = string.Empty;

    public string AnsweringGuidance { get; init; } = string.Empty;

    public IReadOnlyList<string> AcceptedAnswerAliases { get; init; } = [];

    public FlashcardAnalyticsProgressSnapshotResponse Progress { get; init; } = new();

    public IReadOnlyList<FlashcardAnalyticsRecentAnswerResponse> RecentAnswers { get; init; } = [];

    public StudentFlashcardAnalyticsResponse? CurrentAnalytics { get; init; }
}

public record FlashcardAnalyticsProgressSnapshotResponse
{
    public int CorrectCount { get; init; }

    public int WrongCount { get; init; }

    public int TotalAttempts { get; init; }

    public int ConsecutiveCorrectCount { get; init; }

    public int ConsecutiveWrongCount { get; init; }

    public int ReviewCount { get; init; }

    public int LapseCount { get; init; }

    public string LastReviewOutcome { get; init; } = string.Empty;

    public string? LastEvaluationVerdict { get; init; }

    public int? LastQualityScore { get; init; }

    public DateTime? LastReviewedAt { get; init; }

    public DateTime NextReviewAt { get; init; }
}

public record FlashcardAnalyticsRecentAnswerResponse
{
    public string SubmittedAnswer { get; init; } = string.Empty;

    public string ExpectedAnswerSnapshot { get; init; } = string.Empty;

    public int ResponseTimeMs { get; init; }

    public int FinalQualityScore { get; init; }

    public bool WasAcceptedAsCorrect { get; init; }

    public string? Verdict { get; init; }

    public string FeedbackSummary { get; init; } = string.Empty;

    public string SemanticRationale { get; init; } = string.Empty;

    public DateTime AnsweredAt { get; init; }
}
