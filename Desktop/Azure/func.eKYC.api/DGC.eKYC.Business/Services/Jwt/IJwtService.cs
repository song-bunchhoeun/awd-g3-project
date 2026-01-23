using System.Security.Claims;

namespace DGC.eKYC.Business.Services.Jwt;

public interface IJwtService
{
    string GenerateToken(string subject, int expiryMinutes, DateTimeOffset datetime, Dictionary<string, string?> additionalClaims);
    ClaimsPrincipal ValidateToken(string? token);
    T? GetClaimValue<T>(string token, string claimType);
}