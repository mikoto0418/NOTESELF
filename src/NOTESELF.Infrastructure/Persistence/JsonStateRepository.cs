using System.Text.Json;
using NOTESELF.Core.Interfaces;
using NOTESELF.Core.Models;

namespace NOTESELF.Infrastructure.Persistence;

public sealed class JsonStateRepository : IStateRepository
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly StateFileLocator _locator;

    public JsonStateRepository(StateFileLocator locator)
    {
        _locator = locator;
    }

    public async Task<AppState> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = _locator.GetStateFilePath();
        if (!File.Exists(path))
        {
            return new AppState();
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<AppState>(stream, ReadOptions, cancellationToken);
            return state ?? new AppState();
        }
        catch
        {
            return new AppState();
        }
    }

    public async Task SaveAsync(AppState state, CancellationToken cancellationToken = default)
    {
        var path = _locator.GetStateFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, state, WriteOptions, cancellationToken);
    }
}

