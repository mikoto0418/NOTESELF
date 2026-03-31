using NOTESELF.Core.Interfaces;
using NOTESELF.Core.Models;

namespace NOTESELF.Core.Services;

public sealed class NoteService : INoteService
{
    private readonly IStateRepository _repository;
    private readonly StateMigrationService _migrationService;

    public NoteService(IStateRepository repository, StateMigrationService migrationService)
    {
        _repository = repository;
        _migrationService = migrationService;
        State = _migrationService.Normalize(null);
    }

    public AppState State { get; private set; }

    public async Task<AppState> LoadStateAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _repository.LoadAsync(cancellationToken);
        State = _migrationService.Normalize(raw);
        return State;
    }

    public Task SaveStateAsync(CancellationToken cancellationToken = default)
    {
        return _repository.SaveAsync(State, cancellationToken);
    }

    public Task<NoteItem> CreateNoteAsync(string? title = null, bool showWidget = false, CancellationToken cancellationToken = default)
    {
        var note = BuildFreshNote(title, showWidget);
        State.Notes.Insert(0, note);
        NormalizeCurrentState();
        return Task.FromResult(FindNoteOrThrow(note.Id));
    }

    public Task<NoteItem?> UpdateNoteAsync(string noteId, NoteUpdate patch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patch);

        var target = State.Notes.FirstOrDefault(note => note.Id == noteId);
        if (target is null)
        {
            return Task.FromResult<NoteItem?>(null);
        }

        if (patch.Title is not null)
        {
            var trimmed = patch.Title.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                target.Title = trimmed;
            }
        }

        if (patch.Html is not null)
        {
            target.Html = patch.Html;
        }

        if (patch.Markdown is not null)
        {
            target.Markdown = patch.Markdown;
        }

        if (patch.Style is not null)
        {
            target.Style = patch.Style;
        }

        if (patch.AccentColor is not null)
        {
            target.AccentColor = patch.AccentColor;
        }

        if (patch.Pinned.HasValue)
        {
            target.Pinned = patch.Pinned.Value;
        }

        if (patch.Widget is not null)
        {
            target.Widget.Visible = patch.Widget.Visible ?? target.Widget.Visible;
            target.Widget.Frozen = patch.Widget.Frozen ?? target.Widget.Frozen;
            target.Widget.X = patch.Widget.X ?? target.Widget.X;
            target.Widget.Y = patch.Widget.Y ?? target.Widget.Y;
            target.Widget.Width = patch.Widget.Width ?? target.Widget.Width;
            target.Widget.Height = patch.Widget.Height ?? target.Widget.Height;
            target.Widget.Opacity = patch.Widget.Opacity ?? target.Widget.Opacity;
            target.Widget.AlwaysOnTop = patch.Widget.AlwaysOnTop ?? target.Widget.AlwaysOnTop;
        }

        target.UpdatedAt = DateTimeOffset.UtcNow.ToString("O");
        NormalizeCurrentState();
        return Task.FromResult(State.Notes.FirstOrDefault(note => note.Id == noteId));
    }

    public Task<bool> DeleteNoteAsync(string noteId, CancellationToken cancellationToken = default)
    {
        var removed = State.Notes.RemoveAll(note => note.Id == noteId) > 0;
        if (removed)
        {
            NormalizeCurrentState();
        }

        return Task.FromResult(removed);
    }

    public Task<NoteItem?> DuplicateNoteAsync(string noteId, CancellationToken cancellationToken = default)
    {
        var source = State.Notes.FirstOrDefault(note => note.Id == noteId);
        if (source is null)
        {
            return Task.FromResult<NoteItem?>(null);
        }

        var duplicated = BuildFreshNote($"{source.Title}（副本）", showWidget: false);
        duplicated.Html = source.Html;
        duplicated.Markdown = source.Markdown;
        duplicated.Style = source.Style;
        duplicated.AccentColor = source.AccentColor;
        duplicated.Pinned = source.Pinned;
        duplicated.Widget.Opacity = source.Widget.Opacity;
        duplicated.Widget.AlwaysOnTop = source.Widget.AlwaysOnTop;
        duplicated.Widget.Visible = false;
        duplicated.Widget.Frozen = false;
        duplicated.Widget.X = null;
        duplicated.Widget.Y = null;

        State.Notes.Insert(0, duplicated);
        NormalizeCurrentState();
        return Task.FromResult(State.Notes.FirstOrDefault(note => note.Id == duplicated.Id));
    }

    public Task<NoteItem?> TogglePinAsync(string noteId, CancellationToken cancellationToken = default)
    {
        var target = State.Notes.FirstOrDefault(note => note.Id == noteId);
        if (target is null)
        {
            return Task.FromResult<NoteItem?>(null);
        }

        target.Pinned = !target.Pinned;
        target.UpdatedAt = DateTimeOffset.UtcNow.ToString("O");
        NormalizeCurrentState();
        return Task.FromResult(State.Notes.FirstOrDefault(note => note.Id == noteId));
    }

    private NoteItem BuildFreshNote(string? title, bool showWidget)
    {
        var normalized = _migrationService.Normalize(null);
        var seed = normalized.Notes[0];
        seed.Id = Guid.NewGuid().ToString();
        seed.Title = string.IsNullOrWhiteSpace(title) ? "新便签" : title.Trim();
        seed.CreatedAt = DateTimeOffset.UtcNow.ToString("O");
        seed.UpdatedAt = seed.CreatedAt;
        seed.Widget.Visible = showWidget;
        seed.Widget.Frozen = false;
        seed.Widget.AlwaysOnTop = false;
        seed.Widget.Opacity = State.Settings.DefaultOpacity;
        seed.Style = State.Settings.DefaultStyle;
        seed.AccentColor = State.Settings.DefaultAccentColor;
        return seed;
    }

    private NoteItem FindNoteOrThrow(string noteId)
    {
        return State.Notes.First(note => note.Id == noteId);
    }

    private void NormalizeCurrentState()
    {
        State = _migrationService.Normalize(State);
    }
}

