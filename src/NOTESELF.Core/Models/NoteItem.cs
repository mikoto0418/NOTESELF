namespace NOTESELF.Core.Models;

public class NoteItem
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Html { get; set; } = string.Empty;

    public string Markdown { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Style { get; set; } = string.Empty;

    public string AccentColor { get; set; } = string.Empty;

    public bool Pinned { get; set; }

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;

    public WidgetState Widget { get; set; } = new();
}

