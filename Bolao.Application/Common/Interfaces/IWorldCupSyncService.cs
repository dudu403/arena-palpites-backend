using Bolao.Application.WorldCup.SyncWorldCup;

namespace Bolao.Application.Common.Interfaces;

public interface IWorldCupSyncService
{
    Task<SyncWorldCupResponse> SyncAsync(CancellationToken cancellationToken);
}