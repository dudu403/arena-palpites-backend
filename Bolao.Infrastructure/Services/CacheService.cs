using Bolao.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Bolao.Infrastructure.Services;

public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogInformation(
                "CACHE HIT: {CacheKey}",
                key);

            return value;
        }

        _logger.LogInformation(
            "CACHE MISS: {CacheKey}",
            key);

        return default;
    }

    public void Set<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        _memoryCache.Set(
            key,
            value,
            expiration);

        _logger.LogInformation(
            "CACHE SET: {CacheKey}",
            key);
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        _memoryCache.Remove(key);

        _logger.LogInformation(
            "CACHE REMOVE: {CacheKey}",
            key);
    }
}