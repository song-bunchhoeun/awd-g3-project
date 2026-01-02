using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DGC.eKYC.Business.Services.Jwt;

public class JwtService(IConfiguration config) : IJwtService
{
    // Pre-calculate the key object for maximum efficiency in a Singleton
    private readonly SymmetricSecurityKey _signingKey = new(
        Encoding.ASCII.GetBytes(config.GetValue<string>("Jwt:SecretKey")
                                ?? throw new InvalidOperationException("JWT Secret Key is not configured.")));

    private readonly string _issuer = config.GetValue<string>("Jwt:Issuer") ?? "DefaultIssuer";
    private readonly string _audience = config.GetValue<string>("Jwt:Audience") ?? "DefaultAudience";

    public string GenerateToken(string subject, int expiryMinutes, Dictionary<string, string?> additionalClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Standard registered claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Dynamically inject additional parameters
        foreach (var (type, value) in additionalClaims)
        {
            claims.Add(new Claim(type, value ?? string.Empty));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                _signingKey,
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenValid(string token) => GetPrincipalFromToken(token) != null;
}