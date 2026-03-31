namespace NOTESELF.Infrastructure.Persistence;

public sealed class StateFileLocator
{
    private const string StateFileName = "material-notes-state.json";
    private static readonly string[] CandidateFolders =
    [
        "dont-forget-notes",
        "Don't Forget Notes",
        "NOTESELF"
    ];

    public string GetStateFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        foreach (var folder in CandidateFolders)
        {
            var candidate = Path.Combine(appData, folder, StateFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(appData, CandidateFolders[0], StateFileName);
    }
}

