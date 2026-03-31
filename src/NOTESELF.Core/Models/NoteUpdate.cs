namespace NOTESELF.Core.Models;

public class NoteUpdate
{
    public string? Title { get; set; }

    public string? Html { get; set; }

    public string? Markdown { get; set; }

    public string? Style { get; set; }

    public string? AccentColor { get; set; }

    public bool? Pinned { get; set; }

    public WidgetUpdate? Widget { get; set; }
}

