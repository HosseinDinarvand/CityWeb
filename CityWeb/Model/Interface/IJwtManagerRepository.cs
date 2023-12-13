using CityWeb.Auth.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApplication10.Models.Interface
{
    public interface IJwtManagerRepository
    {
        JwtSecurityToken CreateToken(List<Claim> authCliams);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
        string GenerateRefreshToken();
    }
}
