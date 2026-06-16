namespace Bolao.Api.Contracts.Boloes;

public sealed class CreateBolaoRequest
{
    public string Name { get; init; } = string.Empty;

    public string Championship { get; init; } = string.Empty;

    public int ChampionshipExternalId { get; init; }

    public string? Description { get; init; }

    public int MaxParticipants { get; init; }

    public string Privacy { get; init; } = "Private";

    public BolaoRulesRequest Rules { get; init; } = new();
}

public sealed class BolaoRulesRequest
{
    public int ExactScorePoints { get; init; }

    public int WinnerPoints { get; init; }

    public int DrawPoints { get; init; }
}