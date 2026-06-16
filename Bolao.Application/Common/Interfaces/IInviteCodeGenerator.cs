namespace Bolao.Application.Common.Interfaces;

public interface IInviteCodeGenerator
{
    Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default);
}