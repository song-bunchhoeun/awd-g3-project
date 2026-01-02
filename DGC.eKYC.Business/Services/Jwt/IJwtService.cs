using System.Security.Claims;

namespace DGC.eKYC.Business.Services.Jwt;

public interface IJwtService
{
    string GenerateToken(string subject, int expiryMinutes, Dictionary<string, string?> additionalClaims);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    bool IsTokenValid(string token);
}