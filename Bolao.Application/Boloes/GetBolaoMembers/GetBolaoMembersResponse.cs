namespace Bolao.Application.Boloes.GetBolaoMembers;

public sealed class GetBolaoMembersResponse
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}