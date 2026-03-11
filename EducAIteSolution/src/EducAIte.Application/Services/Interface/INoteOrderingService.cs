namespace EducAIte.Application.Services.Interface;

public interface INoteOrderingService
{
    Task<decimal> ComputeAppendLastSequenceAsync(
        string documentSqid,
        CancellationToken cancellationToken = default);

    Task<bool> MoveBetweenAsync(
        long studentId,
        string documentSqid,
        string noteSqid,
        string? previousNoteSqid,
        string? nextNoteSqid,
        CancellationToken cancellationToken = default);

    Task RebalanceAsync(long studentId, string documentSqid, CancellationToken cancellationToken = default);
}
