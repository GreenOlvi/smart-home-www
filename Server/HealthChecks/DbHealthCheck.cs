using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.HealthChecks;

public class DbHealthCheck(IDbContextFactory<SmartHomeDbContext> contextFactory) : IHealthCheck
{
    private readonly IDbContextFactory<SmartHomeDbContext> _contextFactory = contextFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var _ = await db.Database.ExecuteSqlAsync($"SELECT 1", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch(Exception ex)
        {
            return HealthCheckResult.Unhealthy(context.Registration.FailureStatus.ToString(), exception: ex);
        }
    }
}
