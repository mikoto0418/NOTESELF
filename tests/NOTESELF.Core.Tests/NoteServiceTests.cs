using FluentAssertions;
using NOTESELF.Core.Interfaces;
using NOTESELF.Core.Models;
using NOTESELF.Core.Services;

namespace NOTESELF.Core.Tests;

public class NoteServiceTests
{
    private readonly InMemoryStateRepository _repository = new();
    private readonly NoteService _service;

    public NoteServiceTests()
    {
        _service = new NoteService(_repository, new StateMigrationService());
    }

    [Fact]
    public async Task CreateNoteAsync_ShouldInsertNewNoteAtTopUsingDefaults()
    {
        await _service.LoadStateAsync();

        var note = await _service.CreateNoteAsync();

        _service.State.Notes.Should().NotBeEmpty();
        _service.State.Notes[0].Id.Should().Be(note.Id);
        note.Title.Should().Be("新便签");
        note.Style.Should().Be("glass");
        note.AccentColor.Should().Be("#7c4dff");
        note.Widget.Visible.Should().BeFalse();
        note.Widget.Frozen.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNoteAsync_ShouldUpdateContentAndRefreshExcerpt()
    {
        await _service.LoadStateAsync();
        var created = await _service.CreateNoteAsync();

        var updated = await _service.UpdateNoteAsync(
            created.Id,
            new NoteUpdate
            {
                Title = "更新后的标题",
                Html = "<p>新的正文内容</p>",
                Markdown = "新的正文内容",
                Style = "paper",
                AccentColor = "#26c6da",
                Widget = new WidgetUpdate
                {
                    Opacity = 0.88,
                    AlwaysOnTop = true
                }
            });

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("更新后的标题");
        updated.Excerpt.Should().Be("新的正文内容");
        updated.Style.Should().Be("paper");
        updated.AccentColor.Should().Be("#26c6da");
        updated.Widget.Opacity.Should().BeApproximately(0.88, 0.001);
        updated.Widget.AlwaysOnTop.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNoteAsync_ShouldRemoveMatchingNote()
    {
        await _service.LoadStateAsync();
        var created = await _service.CreateNoteAsync();

        var deleted = await _service.DeleteNoteAsync(created.Id);

        deleted.Should().BeTrue();
        _service.State.Notes.Should().NotContain(note => note.Id == created.Id);
    }

    [Fact]
    public async Task DuplicateNoteAsync_ShouldCloneContentAndResetWidgetVisibility()
    {
        await _service.LoadStateAsync();
        var created = await _service.CreateNoteAsync();
        await _service.UpdateNoteAsync(
            created.Id,
            new NoteUpdate
            {
                Title = "原便签",
                Html = "<p>原正文</p>",
                Markdown = "原正文",
                Style = "solid",
                AccentColor = "#ff7043",
                Widget = new WidgetUpdate
                {
                    Visible = true,
                    Frozen = true,
                    X = 100,
                    Y = 120
                }
            });

        var duplicated = await _service.DuplicateNoteAsync(created.Id);

        duplicated.Should().NotBeNull();
        duplicated!.Id.Should().NotBe(created.Id);
        duplicated.Title.Should().Be("原便签（副本）");
        duplicated.Html.Should().Be("<p>原正文</p>");
        duplicated.Markdown.Should().Be("原正文");
        duplicated.Widget.Visible.Should().BeFalse();
        duplicated.Widget.Frozen.Should().BeFalse();
        duplicated.Widget.X.Should().BeNull();
        duplicated.Widget.Y.Should().BeNull();
    }

    [Fact]
    public async Task TogglePinAsync_ShouldTogglePinnedAndResortNotes()
    {
        await _service.LoadStateAsync();
        var first = await _service.CreateNoteAsync("第一条");
        var second = await _service.CreateNoteAsync("第二条");

        var toggled = await _service.TogglePinAsync(first.Id);

        toggled.Should().NotBeNull();
        toggled!.Pinned.Should().BeTrue();
        _service.State.Notes[0].Id.Should().Be(first.Id);
        _service.State.Notes[1].Id.Should().Be(second.Id);
    }

    [Fact]
    public async Task LoadStateAsync_AndSaveStateAsync_ShouldRoundTripThroughRepository()
    {
        var loaded = await _service.LoadStateAsync();
        loaded.Notes.Should().HaveCount(1);

        await _service.CreateNoteAsync("保存测试");
        await _service.SaveStateAsync();

        _repository.LastSavedState.Should().NotBeNull();
        _repository.LastSavedState!.Notes.Should().Contain(note => note.Title == "保存测试");
    }

    private sealed class InMemoryStateRepository : IStateRepository
    {
        private AppState? _state;

        public AppState? LastSavedState { get; private set; }

        public Task<AppState> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_state ?? new AppState());
        }

        public Task SaveAsync(AppState state, CancellationToken cancellationToken = default)
        {
            LastSavedState = state;
            _state = state;
            return Task.CompletedTask;
        }
    }
}

