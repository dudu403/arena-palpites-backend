using Bolao.Application.Predictions.ProcessFinishedMatches;
using Bolao.Application.WorldCup.SyncWorldCup;

namespace Bolao.Api.BackgroundServices;

public sealed class WorldCupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorldCupBackgroundService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public WorldCupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorldCupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "WorldCupBackgroundService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteJobAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao executar WorldCupBackgroundService.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ExecuteJobAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var syncWorldCupUseCase =
            scope.ServiceProvider.GetRequiredService<SyncWorldCupUseCase>();

        var processFinishedMatchesUseCase =
            scope.ServiceProvider.GetRequiredService<ProcessFinishedMatchesUseCase>();

        _logger.LogInformation(
            "Iniciando sincronização automática da Copa do Mundo.");

        var syncResult = await syncWorldCupUseCase.ExecuteAsync(
            new SyncWorldCupCommand(),
            cancellationToken);

        _logger.LogInformation(
            "Sync finalizado. Campeonatos: {Championships}, Times: {Teams}, Jogos: {Matches}, Classificação: {Standings}",
            syncResult.ChampionshipsSynced,
            syncResult.TeamsSynced,
            syncResult.MatchesSynced,
            syncResult.StandingsSynced);

        var processResult = await processFinishedMatchesUseCase.ExecuteAsync(
            new ProcessFinishedMatchesCommand
            {
                MaxMatchesToProcess = 50
            },
            cancellationToken);

        _logger.LogInformation(
            "Processamento finalizado. Partidas: {Matches}, Palpites: {Predictions}",
            processResult.MatchesProcessed,
            processResult.PredictionsProcessed);
    }
}