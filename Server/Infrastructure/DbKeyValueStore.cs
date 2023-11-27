using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Utils.Functional;
using System.Text.Json;

namespace SmartHomeWWW.Server.Infrastructure;

public class DbKeyValueStore(IDbContextFactory<SmartHomeDbContext> dbContextFactory) : IKeyValueStore
{
    private readonly IDbContextFactory<SmartHomeDbContext> _contextFactory = dbContextFactory;
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = false };

    public async Task AddValueAsync(string key, string value, TimeSpan? lifetime = null)
    {
        DateTime? expireAt = lifetime.HasValue ? DateTime.UtcNow.Add(lifetime.Value) : null;
        await using var db = await _contextFactory.CreateDbContextAsync();
        await CleanExpired(db);
        await db.CacheEntries.AddAsync(new CacheStoreEntry
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            ExpireAt = expireAt,
        });
        await db.SaveChangesAsync();
    }

    public Task AddValueAsync<T>(string key, T value, TimeSpan? lifetime = null) => AddValueAsync(key, Serialize(value), lifetime);

    public Task<T?> GetValueAsync<T>(string key) => throw new NotImplementedException();

    public async Task<Option<T>> TryGetValueAsync<T>(string key)
    {
        var now = DateTime.UtcNow;
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entry = await db.CacheEntries.FirstOrDefaultAsync(e => e.Key == key && (e.ExpireAt.HasValue == false || e.ExpireAt > now));
        if (entry is null)
        {
            return new Option<T>.None();
        }

        return Deserialize<T>(entry.Value);
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        var now = DateTime.UtcNow;
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entry = await db.CacheEntries.FirstOrDefaultAsync(e => e.Key == key && (e.ExpireAt.HasValue == false || e.ExpireAt > now));
        return entry is not null;
    }

    private static async Task CleanExpired(SmartHomeDbContext db)
    {
        var now = DateTime.UtcNow;
        var expired = db.CacheEntries.Where(e => e.ExpireAt <= now);
        db.CacheEntries.RemoveRange(expired);
        await db.SaveChangesAsync();
    }

    private string Serialize<T>(T value) => JsonSerializer.Serialize(value, _serializerOptions);

    private Option<T> Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _serializerOptions).ToOption();
}
