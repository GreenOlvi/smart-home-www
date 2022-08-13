using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace SmartHomeWWW.Server.Persistence;

public class MemoryTokenRepository : ITokenRepository
{
    private readonly ConcurrentDictionary<Guid, TokenEntry> _cache = new();
    private readonly ILogger<MemoryTokenRepository> _logger;

    public MemoryTokenRepository(ILogger<MemoryTokenRepository> logger)
    {
        _logger = logger;
    }

    public Task AddToken(Guid id, JwtSecurityToken token)
    {
        _cache.TryAdd(id, new TokenEntry {
            Id = id,
            User = token.Subject,
            ExpirationDate = token.ValidTo,
        });
        _logger.LogDebug("Added new token {Id} valid to {Date}", id, token.ValidTo.ToLocalTime());
        DumpTokens();
        return Task.CompletedTask;
    }

    public Task CancelToken(Guid id)
    {
        _cache.TryRemove(id, out _);
        DumpTokens();
        return Task.CompletedTask;
    }

    public Task<bool> IsActive(Guid id)
    {
        DumpTokens();
        if (!_cache.TryGetValue(id, out var token))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(token.ExpirationDate > DateTime.UtcNow);
    }

    private void DumpTokens()
    {
        _logger.LogDebug("Stored tokens:");
        foreach (var t in _cache.Values)
        {
            _logger.LogDebug("{Id}; {User}; {Date}", t.Id, t.User, t.ExpirationDate.ToLocalTime());
        }
    }

    private record TokenEntry
    {
        public Guid Id { get; init; }
        public string User { get; init; } = string.Empty;
        public DateTime ExpirationDate { get; init; }
    }
}
