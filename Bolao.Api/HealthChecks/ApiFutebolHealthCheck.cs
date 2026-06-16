using Bolao.Infrastructure.ExternalServices.ApiFutebol;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Bolao.Api.HealthChecks;

public sealed class ApiFutebolHealthCheck : IHealthCheck
{
    private readonly ApiFutebolSettings _settings;

    public ApiFutebolHealthCheck(
        IOptions<ApiFutebolSettings> options)
    {
        _settings = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "ApiFutebol BaseUrl não configurada."));
        }

        if (string.IsNullOrWhiteSpace(_settings.Token))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "ApiFutebol Token não configurado."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "ApiFutebol configurada."));
    }
}