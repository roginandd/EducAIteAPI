using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Note;

public sealed class NoteForbiddenException : ForbiddenException
{
    public NoteForbiddenException()
        : base("Note does not belong to the authenticated student.", "note_forbidden")
    {
    }
}
