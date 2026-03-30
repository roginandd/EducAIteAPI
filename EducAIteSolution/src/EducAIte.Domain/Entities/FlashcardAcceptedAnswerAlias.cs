namespace EducAIte.Domain.Entities;

public class FlashcardAcceptedAnswerAlias
{
    public long FlashcardAcceptedAnswerAliasId { get; private set; }
    public long FlashcardId { get; private set; }
    public Flashcard Flashcard { get; private set; } = null!;
    public string Alias { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private FlashcardAcceptedAnswerAlias()
    {
    }

    internal FlashcardAcceptedAnswerAlias(string alias, int order)
    {
        Alias = NormalizeAlias(alias);
        Order = ValidateOrder(order);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Update(string alias, int order)
    {
        Alias = NormalizeAlias(alias);
        Order = ValidateOrder(order);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Accepted answer alias cannot be empty.", nameof(alias));
        }

        string normalized = alias.Trim();
        if (normalized.Length > 500)
        {
            throw new ArgumentException("Accepted answer alias cannot exceed 500 characters.", nameof(alias));
        }

        return normalized;
    }

    private static int ValidateOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentException("Order must not be negative.", nameof(order));
        }

        return order;
    }
}
