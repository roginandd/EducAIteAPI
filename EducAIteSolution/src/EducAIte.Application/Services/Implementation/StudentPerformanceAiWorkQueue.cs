using System.Runtime.CompilerServices;
using System.Threading.Channels;
using EducAIte.Application.Services;
using EducAIte.Application.Services.Interface;

namespace EducAIte.Application.Services.Implementation;

public sealed class StudentPerformanceAiWorkQueue : IStudentPerformanceAiWorkQueue
{
    private readonly Channel<StudentPerformanceAiWorkItem> _channel = Channel.CreateUnbounded<StudentPerformanceAiWorkItem>();

    public ValueTask QueueAsync(StudentPerformanceAiWorkItem workItem, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(workItem, cancellationToken);
    }

    public async IAsyncEnumerable<StudentPerformanceAiWorkItem> DequeueAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out StudentPerformanceAiWorkItem? workItem))
            {
                yield return workItem;
            }
        }
    }
}
