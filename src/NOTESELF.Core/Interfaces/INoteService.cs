using NOTESELF.Core.Models;

namespace NOTESELF.Core.Interfaces;

public interface INoteService
{
    AppState State { get; }

    Task<AppState> LoadStateAsync(CancellationToken cancellationToken = default);

    Task SaveStateAsync(CancellationToken cancellationToken = default);

    Task<NoteItem> CreateNoteAsync(string? title = null, bool showWidget = false, CancellationToken cancellationToken = default);

    Task<NoteItem?> UpdateNoteAsync(string noteId, NoteUpdate patch, CancellationToken cancellationToken = default);

    Task<bool> DeleteNoteAsync(string noteId, CancellationToken cancellationToken = default);

    Task<NoteItem?> DuplicateNoteAsync(string noteId, CancellationToken cancellationToken = default);

    Task<NoteItem?> TogglePinAsync(string noteId, CancellationToken cancellationToken = default);
}

