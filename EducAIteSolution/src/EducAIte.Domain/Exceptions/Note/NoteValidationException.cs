using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Note;

public sealed class NoteValidationException : ValidationException
{
    public NoteValidationException(string message)
        : base(message, "note_validation_error")
    {
    }
}
