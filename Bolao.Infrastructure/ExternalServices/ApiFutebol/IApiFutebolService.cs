namespace Bolao.Infrastructure.ExternalServices.ApiFutebol;

public interface IApiFutebolService
{
    Task<string> GetWorldCupAsync();

    Task<string> GetWorldCupGroupStageAsync();
}