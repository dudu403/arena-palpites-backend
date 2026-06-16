namespace Bolao.Application.Boloes.DeleteBolao;

public sealed class DeleteBolaoResponse
{
    public Guid BolaoId { get; init; }
    public string Message { get; init; } = string.Empty;
}