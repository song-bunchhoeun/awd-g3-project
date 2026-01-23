using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;

namespace DGC.eKYC.Business.Services.Jwt;

public class ApiJwtService(IConfiguration config) : IJwtService
{
    // Pre-calculate the key object for maximum efficiency in a Singleton
    private readonly SymmetricSecurityKey _signingKey = new(
        Encoding.UTF8.GetBytes(config.GetValue<string>("JwtSettings:SecretKey")
                               ?? throw new InvalidOperationException("JWT Secret Key is not configured.")));

    private readonly string _issuer = config.GetValue<string>("JwtSettings:Issuer") ?? "DefaultIssuer";
    private readonly string _audience = config.GetValue<string>("JwtSettings:Audience") ?? "DefaultAudience";
    private const string Scheme = "Bearer";

    public string GenerateToken(string subject, int expiryMinutes, DateTimeOffset datetime, Dictionary<string, string?> additionalClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Standard registered claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, datetime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
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

    /// <summary>
    /// Validates a token and returns a detailed result with specific failure reasons.
    /// This method does not log token contents or secrets.
    /// </summary>
    public ClaimsPrincipal ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new CustomHttpResponseException(
                400,
                new ErrorResponse(
                    "bad_request",
                    "Validation failed for the request.",
                    [new ErrorDetail("malformed_token", "Token is missing or empty.")]));

        var parts = token.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals(Scheme, StringComparison.OrdinalIgnoreCase))
            throw new CustomHttpResponseException(
                401,
                new ErrorResponse(
                    "bad_request",
                    "Validation failed for the request.",
                    [new ErrorDetail(
                        "bad_authorization_header", 
                        "Bearer Authorization Header is required for this endpoint")]));

        var cleanedToken = parts[1];

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(cleanedToken))
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("malformed_token", "Token format is invalid."));

        try
        {
            var principal = tokenHandler.ValidateToken(cleanedToken, new TokenValidationParameters
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

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("expired_token", ex.Message));
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("invalid_issuer", ex.Message));
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("invalid_audience", ex.Message));
        }
        catch (Exception ex) 
            when (ex is SecurityTokenSignatureKeyNotFoundException or SecurityTokenInvalidSignatureException)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("invalid_signature", ex.Message));
        }
        catch (SecurityTokenNoExpirationException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("expired_token", ex.Message));
        }
        catch (SecurityTokenValidationException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("invalid_token", ex.Message));
        }
        catch (ArgumentException ex)
        {
            throw new CustomHttpResponseException(
                401, 
                new ErrorResponse("malformed_token", ex.Message));
        }
        catch (Exception ex)
        {
            throw new CustomHttpResponseException(
                500, 
                new ErrorResponse("internal_validation_error", ex.Message));
        }
    }

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