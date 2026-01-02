using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DGC.eKYC.Business.Services.Jwt;

public class FunctionJwtService : IJwtService
{
    public string GenerateToken(string subject, int expiryMinutes, Dictionary<string, string?> additionalClaims)
        => throw new NotImplementedException();

    public ClaimsPrincipal? GetPrincipalFromToken(string token) => throw new NotImplementedException();

    public bool IsTokenValid(string token) => throw new NotImplementedException();

    /// <summary>
    /// Parses a JWT string to retrieve a claim without requiring a SecretKey.
    /// Useful for inspecting tokens from external providers.
    /// </summary>
    public T? GetClaimValue<T>(string token, string claimType)
    {
        if (string.IsNullOrWhiteSpace(token)) return default;

        // 1. Handle potential "Bearer " prefix found in Authorization headers
        var cleanToken = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? token["Bearer ".Length..].Trim()
            : token;

        var handler = new JwtSecurityTokenHandler();

        // 2. Check if the string is even a valid JWT format before trying to read
        if (!handler.CanReadToken(cleanToken)) return default;

        try
        {
            var jwtToken = handler.ReadJwtToken(cleanToken);

            // 3. Search for the claim (handling both standard and custom types)
            var claim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase));

            if (claim == null) return default;

            // 4. Robust type conversion
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T?)converter.ConvertFromInvariantString(claim.Value);
        }
        catch
        {
            // If the token is malformed or corrupted
            return default;
        }
    }
}