namespace EducAIte.Application.Services.Interface;

public interface ISqidService
{
    string Encode(long id);

    bool TryDecode(string sqid, out long id);
}

