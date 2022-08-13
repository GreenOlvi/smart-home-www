using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartHomeWWW.Server.Controllers;

[Authorize]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly ITokenManager _tokenManager;

    public LoginController(ILogger<LoginController> logger, ITokenManager tokenManager)
    {
        _logger = logger;
        _tokenManager = tokenManager;
    }

    [AllowAnonymous]
    [HttpGet("api/login")]
    public async Task<IActionResult> Login(string user, string password)
    {
        if (user == "szulo" && password == "123")
        {
            var stringToken = await _tokenManager.CreateForUser(user);
            return Ok(stringToken);
        }

        _logger.LogDebug("Failed login for {User}", user);
        return Unauthorized();
    }

    [HttpGet("api/logout")]
    public async Task<IActionResult> Logout()
    {
        if (TokenMiddleware.TryExtractToken(HttpContext, out var token))
        {
            await _tokenManager.CancelToken(token);
            return Ok();
        }
        return BadRequest("No token");
    }
}
