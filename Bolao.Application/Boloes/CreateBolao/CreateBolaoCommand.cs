namespace Bolao.Application.Boloes.CreateBolao;

public sealed class CreateBolaoCommand
{
    public string Name { get; init; } = string.Empty;

    public string Championship { get; init; } = string.Empty;

    public int ChampionshipExternalId { get; init; }

    public string? Description { get; init; }

    public int MaxParticipants { get; init; }

    public string Privacy { get; init; } = "Private";

    public CreateBolaoRulesCommand Rules { get; init; } = new();
}

public sealed class CreateBolaoRulesCommand
{
    public int ExactScorePoints { get; init; }

    public int WinnerPoints { get; init; }

    public int DrawPoints { get; init; }
}