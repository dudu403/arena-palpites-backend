namespace Bolao.Application.Common.Interfaces;

public sealed class TeamLogoFileResult
{
    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = "image/png";
}

public interface ITeamLogoService
{
    Task<TeamLogoFileResult?> GetLogoAsync(
        int externalTeamId,
        CancellationToken cancellationToken);
}