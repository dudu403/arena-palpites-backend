namespace Bolao.Application.Boloes.CreateBolao;

public sealed class CreateBolaoResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Championship { get; set; } = string.Empty;

    public int ChampionshipExternalId { get; set; }

    public string? Description { get; set; }

    public int MaxParticipants { get; set; }

    public string Privacy { get; set; } = string.Empty;

    public string? InviteCode { get; set; }

    public Guid OwnerId { get; set; }

    public CreateBolaoRulesResponse Rules { get; set; } = new();
}

public sealed class CreateBolaoRulesResponse
{
    public int ExactScorePoints { get; set; }

    public int WinnerPoints { get; set; }

    public int DrawPoints { get; set; }
}