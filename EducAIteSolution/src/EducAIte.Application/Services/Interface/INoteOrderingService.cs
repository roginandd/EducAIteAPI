namespace EducAIte.Application.Services.Interface;

public interface INoteOrderingService
{
    Task<decimal> ComputeAppendLastSequenceAsync(
        string documentSqid,
        CancellationToken cancellationToken = default);

    Task<bool> MoveBetweenAsync(
        string documentSqid,
        string noteSqid,
        string? previousNoteSqid,
        string? nextNoteSqid,
        CancellationToken cancellationToken = default);

    Task RebalanceAsync(string documentSqid, CancellationToken cancellationToken = default);
}
