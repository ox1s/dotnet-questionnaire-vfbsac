using System.Net;
using Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Caching;

internal sealed class RedisCacheService(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();


    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            RedisValue value = await _database.StringGetAsync(key);

            if (!value.HasValue)
            {
                return null;
            }

            string? stringValue = value;

            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(stringValue);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting cache key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            string serializedValue = JsonSerializer.Serialize(value);

            if (expiration.HasValue)
            {
                await _database.StringSetAsync(key, serializedValue, expiration.Value);
            }
            else
            {
                await _database.StringSetAsync(key, serializedValue);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting cache key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache key {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            // Исправлено: доступ по индексу [0]
            EndPoint[] endpoints = connectionMultiplexer.GetEndPoints();
            IServer server = connectionMultiplexer.GetServer(endpoints[0]);

            IAsyncEnumerable<RedisKey> keys = server.KeysAsync(pattern: pattern);

            await foreach (RedisKey key in keys.WithCancellation(cancellationToken))
            {
                await _database.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache keys by pattern {Pattern}", pattern);
        }
    }
}
