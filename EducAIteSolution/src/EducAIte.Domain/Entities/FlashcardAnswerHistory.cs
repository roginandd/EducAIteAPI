namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class FlashcardAnswerHistory
{
    public long FlashcardAnswerHistoryId { get; private set; }
    public long? FlashcardSessionId { get; private set; }
    public FlashcardSession? FlashcardSession { get; private set; }
    public long? FlashcardSessionItemId { get; private set; }
    public FlashcardSessionItem? FlashcardSessionItem { get; private set; }
    public long StudentFlashcardId { get; private set; }
    public StudentFlashcard StudentFlashcard { get; private set; } = null!;
    public string SubmittedAnswer { get; private set; } = string.Empty;
    public string ExpectedAnswerSnapshot { get; private set; } = string.Empty;
    public int ResponseTimeMs { get; private set; }
    public int? AiQualityScore { get; private set; }
    public int? FallbackQualityScore { get; private set; }
    public int FinalQualityScore { get; private set; }
    public bool WasAcceptedAsCorrect { get; private set; }
    public DateTime AnsweredAt { get; private set; }
    public AnswerScoringSource ScoringSource { get; private set; }
    public FlashcardAnswerEvaluation? Evaluation { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private FlashcardAnswerHistory()
    {
    }

    public static FlashcardAnswerHistory Create(
        long? flashcardSessionId,
        long? flashcardSessionItemId,
        long studentFlashcardId,
        string submittedAnswer,
        string expectedAnswerSnapshot,
        int responseTimeMs,
        int? aiQualityScore,
        int? fallbackQualityScore,
        int finalQualityScore,
        bool wasAcceptedAsCorrect,
        AnswerScoringSource scoringSource,
        DateTime answeredAt)
    {
        return new FlashcardAnswerHistory
        {
            FlashcardSessionId = flashcardSessionId,
            FlashcardSessionItemId = flashcardSessionItemId,
            StudentFlashcardId = ValidatePositiveId(studentFlashcardId, nameof(studentFlashcardId)),
            SubmittedAnswer = NormalizeText(submittedAnswer, nameof(submittedAnswer), 2000),
            ExpectedAnswerSnapshot = NormalizeText(expectedAnswerSnapshot, nameof(expectedAnswerSnapshot), 2000),
            ResponseTimeMs = ValidateNonNegative(responseTimeMs, nameof(responseTimeMs)),
            AiQualityScore = ValidateOptionalScore(aiQualityScore, nameof(aiQualityScore)),
            FallbackQualityScore = ValidateOptionalScore(fallbackQualityScore, nameof(fallbackQualityScore)),
            FinalQualityScore = ValidateScore(finalQualityScore, nameof(finalQualityScore)),
            WasAcceptedAsCorrect = wasAcceptedAsCorrect,
            AnsweredAt = answeredAt.ToUniversalTime(),
            ScoringSource = scoringSource,
            CreatedAt = answeredAt.ToUniversalTime(),
            UpdatedAt = answeredAt.ToUniversalTime()
        };
    }

    public void AttachEvaluation(FlashcardAnswerEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        Evaluation = evaluation;
        UpdatedAt = DateTime.UtcNow;
    }

    private static long ValidatePositiveId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", paramName);
        }

        return id;
    }

    private static int ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentException("Value must not be negative.", paramName);
        }

        return value;
    }

    private static int ValidateScore(int value, string paramName)
    {
        if (value is < 0 or > 5)
        {
            throw new ArgumentException("Score must be between 0 and 5.", paramName);
        }

        return value;
    }

    private static int? ValidateOptionalScore(int? value, string paramName)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return ValidateScore(value.Value, paramName);
    }

    private static string NormalizeText(string value, string paramName, int maxLength)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }

        return normalized;
    }
}
