namespace EducAIte.Domain.Entities;

using EducAIte.Domain.Enum;

public class FlashcardAnswerEvaluation
{
    public long FlashcardAnswerEvaluationId { get; private set; }
    public long FlashcardAnswerHistoryId { get; private set; }
    public FlashcardAnswerHistory FlashcardAnswerHistory { get; private set; } = null!;
    public FlashcardAnswerVerdict Verdict { get; private set; }
    public bool AcceptedAsCorrect { get; private set; }
    public int QualityScore { get; private set; }
    public string FeedbackSummary { get; private set; } = string.Empty;
    public string SemanticRationale { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private FlashcardAnswerEvaluation()
    {
    }

    public FlashcardAnswerEvaluation(
        FlashcardAnswerVerdict verdict,
        bool acceptedAsCorrect,
        int qualityScore,
        string feedbackSummary,
        string semanticRationale)
    {
        Apply(verdict, acceptedAsCorrect, qualityScore, feedbackSummary, semanticRationale);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        FlashcardAnswerVerdict verdict,
        bool acceptedAsCorrect,
        int qualityScore,
        string feedbackSummary,
        string semanticRationale)
    {
        Apply(verdict, acceptedAsCorrect, qualityScore, feedbackSummary, semanticRationale);
        UpdatedAt = DateTime.UtcNow;
    }

    private void Apply(
        FlashcardAnswerVerdict verdict,
        bool acceptedAsCorrect,
        int qualityScore,
        string feedbackSummary,
        string semanticRationale)
    {
        Verdict = verdict;
        AcceptedAsCorrect = acceptedAsCorrect;
        QualityScore = ValidateQualityScore(qualityScore);
        FeedbackSummary = NormalizeText(feedbackSummary, nameof(feedbackSummary), 1000, allowEmpty: false);
        SemanticRationale = NormalizeText(semanticRationale, nameof(semanticRationale), 2000, allowEmpty: true);
    }

    private static int ValidateQualityScore(int qualityScore)
    {
        if (qualityScore is < 0 or > 5)
        {
            throw new ArgumentException("Quality score must be between 0 and 5.", nameof(qualityScore));
        }

        return qualityScore;
    }

    private static string NormalizeText(string value, string paramName, int maxLength, bool allowEmpty)
    {
        string normalized = (value ?? string.Empty).Trim();
        if (!allowEmpty && normalized.Length == 0)
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }

        return normalized;
    }
}
