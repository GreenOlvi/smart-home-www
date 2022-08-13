using Microsoft.Extensions.Primitives;

namespace SmartHomeWWW.Server;

public class TokenMiddleware : IMiddleware
{
    private readonly ILogger<TokenMiddleware> _logger;
    private readonly ITokenManager _tokenManager;

    public TokenMiddleware(ILogger<TokenMiddleware> logger, ITokenManager tokenManager)
    {
        _logger = logger;
        _tokenManager = tokenManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // This means we are not running on Authorized endpoint (TODO find a way to not need this check)
        if (context.User.Identity is null || !context.User.Identity.IsAuthenticated)
        {
            await next(context);
            return;
        }

        if (TryExtractToken(context, out var current) && await _tokenManager.IsTokenActive(current))
        {
            await next(context);
            return;
        }
        _logger.LogDebug("Use of inactive token {User} {Token}", context.User.Identity?.Name, current);
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    public static bool TryExtractToken(HttpContext context, out string token)
    {
        var header = context.Request.Headers["authorization"];
        if (header == StringValues.Empty)
        {
            token = string.Empty;
            return false;
        }

        if (header.Count > 1)
        {
            token = string.Empty;
            return false;
        }

        token = header.Single().Split(" ").Last();
        return true;
    }
}
