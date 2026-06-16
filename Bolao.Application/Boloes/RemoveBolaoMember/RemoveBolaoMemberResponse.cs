namespace Bolao.Application.Boloes.RemoveBolaoMember;

public sealed class RemoveBolaoMemberResponse
{
    public Guid BolaoId { get; init; }
    public Guid RemovedUserId { get; init; }
    public string Message { get; init; } = string.Empty;
}