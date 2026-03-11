using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class NoteMappingExtensions
{
    public static NoteResponse ToResponse(this Note note) => note.Adapt<NoteResponse>();

    public static Note ToEntity(this CreateNoteRequest request)
    {
        return new Note(
            request.Name,
            request.NoteContent,
            request.DocumentId,
            request.SequenceNumber);
    }

    public static void ApplyToEntity(this UpdateNoteRequest request, Note note)
    {
        note.UpdateDetails(
            request.Name,
            request.NoteContent,
            request.DocumentId,
            request.SequenceNumber);
    }
}
