namespace Bolao.Application.Boloes.RemoveBolaoMember;

public sealed class RemoveBolaoMemberCommand
{
    public Guid BolaoId { get; init; }
    public Guid UserId { get; init; }
}