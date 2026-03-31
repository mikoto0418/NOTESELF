using System.Text.Json;
using FluentAssertions;
using NOTESELF.Core.Models;
using NOTESELF.Core.Services;

namespace NOTESELF.Core.Tests;

public class StateMigrationTests
{
    private readonly StateMigrationService _service = new();

    [Fact]
    public void Normalize_WhenInputIsNull_ShouldCreateDefaultStateWithOneWelcomeNote()
    {
        var result = _service.Normalize(null);

        result.Should().NotBeNull();
        result.Settings.Should().NotBeNull();
        result.Notes.Should().HaveCount(1);
        result.Notes[0].Title.Should().Be("欢迎使用 NOTESELF");
        result.Notes[0].Widget.Width.Should().BeGreaterThanOrEqualTo(260);
        result.Notes[0].Widget.Height.Should().BeGreaterThanOrEqualTo(180);
    }

    [Fact]
    public void Normalize_WhenLoadingValidElectronJson_ShouldPreserveSettingsAndNotes()
    {
        const string json = """
        {
          "settings": {
            "themeMode": "dark",
            "launchAtLogin": true,
            "startHidden": false,
            "closeToTray": false,
            "hideMainWindowTaskbar": true,
            "hideWidgetTaskbar": false,
            "defaultStyle": "paper",
            "defaultAccentColor": "#26c6da",
            "defaultOpacity": 0.88
          },
          "notes": [
            {
              "id": "note-1",
              "title": "旧便签",
              "html": "<p>Hello</p>",
              "markdown": "Hello",
              "excerpt": "",
              "style": "solid",
              "accentColor": "#ff7043",
              "pinned": true,
              "createdAt": "2026-03-30T10:00:00.000Z",
              "updatedAt": "2026-03-31T10:00:00.000Z",
              "widget": {
                "visible": true,
                "frozen": false,
                "x": 100,
                "y": 150,
                "width": 420,
                "height": 320,
                "opacity": 0.9,
                "alwaysOnTop": true
              }
            }
          ]
        }
        """;

        var raw = JsonSerializer.Deserialize<AppState>(json, JsonOptions);

        var result = _service.Normalize(raw);

        result.Settings.ThemeMode.Should().Be("dark");
        result.Settings.LaunchAtLogin.Should().BeTrue();
        result.Settings.DefaultStyle.Should().Be("paper");
        result.Notes.Should().HaveCount(1);
        result.Notes[0].Id.Should().Be("note-1");
        result.Notes[0].Widget.Visible.Should().BeTrue();
        result.Notes[0].Widget.AlwaysOnTop.Should().BeTrue();
    }

    [Fact]
    public void Normalize_WhenWidgetValuesAreMissing_ShouldBackfillWidgetDefaults()
    {
        var raw = new AppState
        {
            Settings = new AppSettings(),
            Notes =
            [
                new NoteItem
                {
                    Id = "note-2",
                    Title = "缺字段便签",
                    Html = "<p>Body</p>",
                    Markdown = "Body",
                    Widget = new WidgetState
                    {
                        Width = 120,
                        Height = 90
                    }
                }
            ]
        };

        var result = _service.Normalize(raw);

        result.Notes.Should().HaveCount(1);
        result.Notes[0].Widget.Visible.Should().BeFalse();
        result.Notes[0].Widget.Frozen.Should().BeFalse();
        result.Notes[0].Widget.Width.Should().Be(260);
        result.Notes[0].Widget.Height.Should().Be(180);
        result.Notes[0].Widget.Opacity.Should().BeApproximately(0.96, 0.001);
        result.Notes[0].AccentColor.Should().Be("#7c4dff");
        result.Notes[0].Style.Should().Be("glass");
    }

    [Fact]
    public void Normalize_ShouldSortPinnedFirstThenUpdatedAtDescending()
    {
        var raw = new AppState
        {
            Settings = new AppSettings(),
            Notes =
            [
                new NoteItem
                {
                    Id = "note-a",
                    Title = "普通旧便签",
                    Html = "<p>A</p>",
                    Markdown = "A",
                    Pinned = false,
                    UpdatedAt = "2026-03-30T08:00:00.000Z"
                },
                new NoteItem
                {
                    Id = "note-b",
                    Title = "普通新便签",
                    Html = "<p>B</p>",
                    Markdown = "B",
                    Pinned = false,
                    UpdatedAt = "2026-03-31T08:00:00.000Z"
                },
                new NoteItem
                {
                    Id = "note-c",
                    Title = "置顶便签",
                    Html = "<p>C</p>",
                    Markdown = "C",
                    Pinned = true,
                    UpdatedAt = "2026-03-29T08:00:00.000Z"
                }
            ]
        };

        var result = _service.Normalize(raw);

        result.Notes.Select(note => note.Id)
            .Should()
            .ContainInOrder("note-c", "note-b", "note-a");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

