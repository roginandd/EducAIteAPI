using EducAIte.Application.Services.Interface;
using Sqids;

namespace EducAIte.Application.Services.Implementation;

public sealed class SqidService : ISqidService
{
    private readonly SqidsEncoder<long> _sqids = new(new SqidsOptions
    {
        MinLength = 8
    });

    public string Encode(long id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID must be greater than zero.", nameof(id));
        }

        return _sqids.Encode(id);
    }

    public bool TryDecode(string sqid, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(sqid))
        {
            return false;
        }

        long[] values = _sqids.Decode(sqid).ToArray();
        if (values.Length != 1 || values[0] <= 0)
        {
            return false;
        }

        id = values[0];
        return true;
    }
}

