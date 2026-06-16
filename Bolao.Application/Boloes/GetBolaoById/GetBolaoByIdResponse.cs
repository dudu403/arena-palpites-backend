namespace Bolao.Application.Boloes.GetBolaoById;

public sealed class GetBolaoByIdResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Championship { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int MaxParticipants { get; init; }

    public string Privacy { get; init; } = string.Empty;

    public string? InviteCode { get; init; }

    public Guid OwnerId { get; init; }

    public string Role { get; init; } = string.Empty;

    public GetBolaoByIdRulesResponse Rules { get; init; } = new();
}

public sealed class GetBolaoByIdRulesResponse
{
    public int ExactScorePoints { get; init; }

    public int WinnerPoints { get; init; }

    public int DrawPoints { get; init; }
}