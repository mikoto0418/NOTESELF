namespace NOTESELF.Core.Models;

public class AppState
{
    public AppSettings Settings { get; set; } = new();

    public List<NoteItem> Notes { get; set; } = [];
}

