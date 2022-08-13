using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartHomeWWW.Server;

public class TokenManager : ITokenManager
{
    private readonly ITokenRepository _repository;
    private readonly JwtConfig _config;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenManager(ITokenRepository repository, JwtConfig config)
    {
        _repository = repository;
        _config = config;
    }

    public Task<bool> IsTokenActive(string tokenString)
    {
        var token = _tokenHandler.ReadJwtToken(tokenString);
        if (!Guid.TryParse(token.Id, out var id))
        {
            return Task.FromResult(false);
        }
        return _repository.IsActive(id);
    }

    public async Task<string> CreateForUser(string user)
    {
        var userId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var issuer = _config.Issuer;
        var audience = _config.Audience;
        var key = Encoding.UTF8.GetBytes(_config.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user),
                new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
        };

        var token = _tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        await _repository.AddToken(tokenId, token);
        var jwtToken = _tokenHandler.WriteToken(token);
        return jwtToken;
    }

    public Task CancelToken(string tokenString)
    {
        var token = _tokenHandler.ReadJwtToken(tokenString);
        if (!Guid.TryParse(token.Id, out var id))
        {
            return Task.CompletedTask;
        }
        return _repository.CancelToken(id);
    }
}

public interface ITokenManager
{
    Task<bool> IsTokenActive(string tokenString);
    Task<string> CreateForUser(string user);
    Task CancelToken(string token);
}
