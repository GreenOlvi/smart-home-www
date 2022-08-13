using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace SmartHomeWWW.Server.Persistence;

public interface ITokenRepository
{
    Task AddToken(Guid id, JwtSecurityToken token);
    Task<bool> IsActive(Guid id);
    Task CancelToken(Guid id);
}
