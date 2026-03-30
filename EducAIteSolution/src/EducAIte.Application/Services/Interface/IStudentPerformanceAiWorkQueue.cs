using EducAIte.Application.Services;

namespace EducAIte.Application.Services.Interface;

public interface IStudentPerformanceAiWorkQueue
{
    ValueTask QueueAsync(StudentPerformanceAiWorkItem workItem, CancellationToken cancellationToken = default);

    IAsyncEnumerable<StudentPerformanceAiWorkItem> DequeueAllAsync(CancellationToken cancellationToken = default);
}
