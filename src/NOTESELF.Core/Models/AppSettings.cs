namespace NOTESELF.Core.Models;

public class AppSettings
{
    public string ThemeMode { get; set; } = "system";

    public bool LaunchAtLogin { get; set; } = false;

    public bool StartHidden { get; set; } = true;

    public bool CloseToTray { get; set; } = true;

    public bool HideMainWindowTaskbar { get; set; } = false;

    public bool HideWidgetTaskbar { get; set; } = true;

    public string DefaultStyle { get; set; } = "glass";

    public string DefaultAccentColor { get; set; } = "#7c4dff";

    public double DefaultOpacity { get; set; } = 0.96d;
}

