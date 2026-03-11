using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class NoteMappingExtensions
{
    public static NoteResponse ToResponse(this Note note) => note.Adapt<NoteResponse>();

    public static Note ToEntity(this CreateNoteRequest request, long documentId, decimal sequenceNumber)
    {
        return new Note(
            request.Name,
            request.NoteContent,
            documentId,
            sequenceNumber);
    }

    public static void ApplyToEntity(this UpdateNoteRequest request, Note note, long documentId)
    {
        note.UpdateDetails(
            request.Name,
            request.NoteContent,
            documentId);
    }
}
