using Bolao.Application.Common.Interfaces;

namespace Bolao.Application.WorldCup.SyncWorldCup;

public sealed class SyncWorldCupUseCase
{
    private readonly IWorldCupSyncService _worldCupSyncService;

    public SyncWorldCupUseCase(
        IWorldCupSyncService worldCupSyncService)
    {
        _worldCupSyncService = worldCupSyncService;
    }

    public async Task<SyncWorldCupResponse> ExecuteAsync(
        SyncWorldCupCommand command,
        CancellationToken cancellationToken)
    {
        return await _worldCupSyncService.SyncAsync(cancellationToken);
    }
}