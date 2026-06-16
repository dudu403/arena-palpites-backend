namespace Bolao.Application.WorldCup.SyncWorldCup;

public sealed class SyncWorldCupResponse
{
    public int ChampionshipsSynced { get; set; }
    public int TeamsSynced { get; set; }
    public int MatchesSynced { get; set; }
    public int StandingsSynced { get; set; }
}