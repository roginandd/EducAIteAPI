using EducAIte.Application.DTOs.Request;
using EducAIte.Application.DTOs.Response;
using EducAIte.Application.Services.Interface;
using EducAIte.Domain.Entities;
using Mapster;

namespace EducAIte.Application.Extensions.MappingExtensions;

public static class NoteMappingExtensions
{
    public static NoteResponse ToResponse(this Note note, ISqidService sqidService)
    {
        return note
            .BuildAdapter()
            .AddParameters("sqidService", sqidService)
            .AdaptToType<NoteResponse>();
    }

    public static Note ToEntity(this CreateNoteRequest request, long documentId, decimal sequenceNumber)
    {
        return new Note(
            request.Name,
            request.NoteContent,
            documentId,
            sequenceNumber);
    }

    public static void ApplyToEntity(this UpdateNoteRequest request, Note note)
    {
        note.UpdateDetails(
            request.Name,
            request.NoteContent);
    }
}
