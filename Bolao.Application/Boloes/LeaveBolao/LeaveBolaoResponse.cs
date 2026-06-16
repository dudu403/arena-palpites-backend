namespace Bolao.Application.Boloes.LeaveBolao;

public sealed class LeaveBolaoResponse
{
    public Guid BolaoId { get; init; }
    public string Message { get; init; } = string.Empty;
}