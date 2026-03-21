using EducAIte.Domain.Exceptions.Base;

namespace EducAIte.Domain.Exceptions.Note;

public sealed class NoteNotFoundException : NotFoundException
{
    public NoteNotFoundException(long noteId)
        : base($"Note with ID {noteId} was not found.", "note_not_found")
    {
    }
}
