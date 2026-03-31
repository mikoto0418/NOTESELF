using NOTESELF.Core.Models;

namespace NOTESELF.Core.Interfaces;

public interface IStateRepository
{
    Task<AppState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppState state, CancellationToken cancellationToken = default);
}

