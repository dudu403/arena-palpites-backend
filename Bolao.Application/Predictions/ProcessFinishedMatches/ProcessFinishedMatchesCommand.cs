namespace Bolao.Application.Predictions.ProcessFinishedMatches;

public sealed class ProcessFinishedMatchesCommand
{
    public int MaxMatchesToProcess { get; set; } = 50;
}