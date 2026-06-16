namespace Bolao.Application.Boloes.GetMyPools;

public sealed class GetMyPoolsResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<MyPoolSummaryResponse> Items { get; init; } = [];
}

public sealed class MyPoolSummaryResponse
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