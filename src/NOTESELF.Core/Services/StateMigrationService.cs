using System.Text.RegularExpressions;
using NOTESELF.Core.Models;

namespace NOTESELF.Core.Services;

public sealed class StateMigrationService
{
    private const int MinWidgetWidth = 260;
    private const int MinWidgetHeight = 180;
    private const string DefaultStyle = "glass";
    private const string DefaultAccentColor = "#7c4dff";
    private const double DefaultOpacity = 0.96d;
    private const string EmptyNoteTitle = "未命名便签";
    private const string EmptyExcerpt = "空白便签";
    private static readonly HashSet<string> ValidStyles =
    [
        "solid",
        "glass",
        "paper",
        "minimal"
    ];

    public AppState Normalize(AppState? raw)
    {
        var draft = raw ?? new AppState();
        var notes = draft.Notes?.Select(NormalizeNote).ToList() ?? [];
        if (notes.Count == 0)
        {
            notes.Add(CreateDefaultNote());
        }

        notes = notes
            .OrderByDescending(note => note.Pinned)
            .ThenByDescending(note => ParseDate(note.UpdatedAt))
            .ToList();

        return new AppState
        {
            Settings = NormalizeSettings(draft.Settings),
            Notes = notes
        };
    }

    private static AppSettings NormalizeSettings(AppSettings? settings)
    {
        if (settings is null)
        {
            return new AppSettings();
        }

        return new AppSettings
        {
            ThemeMode = string.IsNullOrWhiteSpace(settings.ThemeMode) ? "system" : settings.ThemeMode,
            LaunchAtLogin = settings.LaunchAtLogin,
            StartHidden = settings.StartHidden,
            CloseToTray = settings.CloseToTray,
            HideMainWindowTaskbar = settings.HideMainWindowTaskbar,
            HideWidgetTaskbar = settings.HideWidgetTaskbar,
            DefaultStyle = string.IsNullOrWhiteSpace(settings.DefaultStyle) ? DefaultStyle : settings.DefaultStyle,
            DefaultAccentColor = string.IsNullOrWhiteSpace(settings.DefaultAccentColor) ? DefaultAccentColor : settings.DefaultAccentColor,
            DefaultOpacity = settings.DefaultOpacity <= 0 ? DefaultOpacity : settings.DefaultOpacity
        };
    }

    private static NoteItem NormalizeNote(NoteItem? input)
    {
        var seed = CreateDefaultNote();
        var note = new NoteItem
        {
            Id = string.IsNullOrWhiteSpace(input?.Id) ? seed.Id : input.Id,
            Title = input?.Title ?? string.Empty,
            Html = string.IsNullOrWhiteSpace(input?.Html) ? seed.Html : input!.Html,
            Markdown = string.IsNullOrWhiteSpace(input?.Markdown) ? seed.Markdown : input!.Markdown,
            Excerpt = input?.Excerpt ?? string.Empty,
            Style = input?.Style ?? string.Empty,
            AccentColor = input?.AccentColor ?? string.Empty,
            Pinned = input?.Pinned ?? false,
            CreatedAt = string.IsNullOrWhiteSpace(input?.CreatedAt) ? seed.CreatedAt : input!.CreatedAt,
            UpdatedAt = string.IsNullOrWhiteSpace(input?.UpdatedAt)
                ? (string.IsNullOrWhiteSpace(input?.CreatedAt) ? seed.UpdatedAt : input!.CreatedAt)
                : input!.UpdatedAt,
            Widget = BuildWidget(input?.Widget)
        };

        note.Excerpt = ToExcerpt(note);
        note.Title = string.IsNullOrWhiteSpace(note.Title)
            ? Truncate(note.Excerpt, 24, fallback: EmptyNoteTitle)
            : note.Title.Trim();
        note.Style = ValidStyles.Contains(note.Style) ? note.Style : DefaultStyle;
        note.AccentColor = string.IsNullOrWhiteSpace(note.AccentColor) ? DefaultAccentColor : note.AccentColor;

        return note;
    }

    private static NoteItem CreateDefaultNote()
    {
        var now = DateTimeOffset.UtcNow.ToString("O");
        const string markdown = """
        # 欢迎使用 NOTESELF

        - 在左侧列表管理便签
        - 用编辑器修改正文内容
        - 后续可以显示到桌面形成悬浮便签
        - 冻结后窗口将支持鼠标穿透
        """;
        const string html = "<h1>欢迎使用 NOTESELF</h1><p>这是你的第一张桌面便签。</p><ul><li>在左侧列表管理便签</li><li>用编辑器修改正文内容</li><li>后续可以显示到桌面形成悬浮便签</li><li>冻结后窗口将支持鼠标穿透</li></ul>";

        var note = new NoteItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = "欢迎使用 NOTESELF",
            Html = html,
            Markdown = markdown,
            Excerpt = string.Empty,
            Style = DefaultStyle,
            AccentColor = DefaultAccentColor,
            Pinned = false,
            CreatedAt = now,
            UpdatedAt = now,
            Widget = BuildWidget(null)
        };

        note.Excerpt = ToExcerpt(note);
        return note;
    }

    private static WidgetState BuildWidget(WidgetState? partial)
    {
        return new WidgetState
        {
            Visible = partial?.Visible ?? false,
            Frozen = partial?.Frozen ?? false,
            X = partial?.X,
            Y = partial?.Y,
            Width = Math.Max(MinWidgetWidth, partial?.Width > 0 ? partial.Width : 360),
            Height = Math.Max(MinWidgetHeight, partial?.Height > 0 ? partial.Height : 260),
            Opacity = partial?.Opacity > 0 ? partial.Opacity : DefaultOpacity,
            AlwaysOnTop = partial?.AlwaysOnTop ?? false
        };
    }

    private static string ToExcerpt(NoteItem note)
    {
        var source = StripMarkdown(note.Markdown);
        if (string.IsNullOrWhiteSpace(source))
        {
            source = StripHtml(note.Html);
        }

        return string.IsNullOrWhiteSpace(source)
            ? EmptyExcerpt
            : Truncate(source, 120, EmptyExcerpt);
    }

    private static string StripHtml(string? html)
    {
        var value = html ?? string.Empty;
        value = Regex.Replace(value, "<style[\\s\\S]*?>[\\s\\S]*?<\\/style>", " ", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<script[\\s\\S]*?>[\\s\\S]*?<\\/script>", " ", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<[^>]+>", " ");
        value = value
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase)
            .Replace("&lt;", "<", StringComparison.OrdinalIgnoreCase)
            .Replace("&gt;", ">", StringComparison.OrdinalIgnoreCase);
        value = Regex.Replace(value, "\\s+", " ");
        return value.Trim();
    }

    private static string StripMarkdown(string? markdown)
    {
        var value = markdown ?? string.Empty;
        value = Regex.Replace(value, "```[\\s\\S]*?```", " ");
        value = Regex.Replace(value, "`([^`]+)`", "$1");
        value = Regex.Replace(value, "!\\[[^\\]]*\\]\\([^\\)]*\\)", " ");
        value = Regex.Replace(value, "\\[([^\\]]+)\\]\\([^\\)]*\\)", "$1");
        value = Regex.Replace(value, "^\\s{0,3}[#>*+-]\\s?", string.Empty, RegexOptions.Multiline);
        value = Regex.Replace(value, "^\\s{0,3}\\d+\\.\\s?", string.Empty, RegexOptions.Multiline);
        value = Regex.Replace(value, "[\\*_~]", string.Empty);
        value = Regex.Replace(value, "\\s+", " ");
        return value.Trim();
    }

    private static DateTimeOffset ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : DateTimeOffset.MinValue;
    }

    private static string Truncate(string? value, int maxLength, string fallback)
    {
        var actual = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return actual.Length <= maxLength ? actual : actual[..maxLength];
    }
}

