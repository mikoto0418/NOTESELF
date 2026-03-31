namespace NOTESELF.Core.Models;

public class WidgetState
{
    public bool Visible { get; set; }

    public bool Frozen { get; set; }

    public int? X { get; set; }

    public int? Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double Opacity { get; set; }

    public bool AlwaysOnTop { get; set; }
}

