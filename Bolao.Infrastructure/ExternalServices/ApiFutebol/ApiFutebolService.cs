using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Bolao.Infrastructure.ExternalServices.ApiFutebol;

public sealed class ApiFutebolService : IApiFutebolService
{
    private readonly HttpClient _httpClient;
    private readonly ApiFutebolSettings _settings;

    public ApiFutebolService(
        HttpClient httpClient,
        IOptions<ApiFutebolSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<string> GetWorldCupAsync()
    {
        return await SendRequest("/campeonatos/72");
    }

    public async Task<string> GetWorldCupGroupStageAsync()
    {
        return await SendRequest("/campeonatos/72/fases/886");
    }

    private async Task<string> SendRequest(string endpoint)
    {
        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}{endpoint}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _settings.Token);

        var response =
            await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}