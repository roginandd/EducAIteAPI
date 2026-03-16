using EducAIte.Domain.Entities;

namespace EducAIteSolution.Tests.Unit.Features.Notes;

public class NoteEntityTests
{
    [Fact]
    public void AddFlashcards_WithValidFlashcards_AddsAllToNote()
    {
        Note note = CreateNote(noteId: 42, documentId: 7);
        Flashcard first = new("Question 1", "Answer 1", 42);
        Flashcard second = new("Question 2", "Answer 2", 42);
        DateTime previousUpdatedAt = note.UpdatedAt;

        note.AddFlashcards([first, second]);

        Assert.Equal(2, note.Flashcards.Count);
        Assert.Contains(first, note.Flashcards);
        Assert.Contains(second, note.Flashcards);
        Assert.Same(note, first.Note);
        Assert.Same(note, second.Note);
        Assert.True(note.UpdatedAt >= previousUpdatedAt);
    }

    [Fact]
    public void AddFlashcards_WithDifferentNoteId_ThrowsWithoutPartialMutation()
    {
        Note note = CreateNote(noteId: 42, documentId: 7);
        Flashcard valid = new("Question 1", "Answer 1", 42);
        Flashcard invalid = new("Question 2", "Answer 2", 999);

        Assert.Throws<InvalidOperationException>(() => note.AddFlashcards([valid, invalid]));
        Assert.Empty(note.Flashcards);
        Assert.Null(valid.Note);
    }

    private static Note CreateNote(long noteId, long documentId)
    {
        Note note = new("Linked Note", "Note content", documentId, 1m);
        typeof(Note).GetProperty(nameof(Note.NoteId))!.SetValue(note, noteId);
        return note;
    }
}
