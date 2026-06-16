namespace Bolao.Application.Boloes.JoinBolao;

public sealed class JoinBolaoResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Championship { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MaxParticipants { get; init; }
    public string Privacy { get; init; } = string.Empty;
    public string? InviteCode { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}