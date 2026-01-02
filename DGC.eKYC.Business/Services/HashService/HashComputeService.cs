using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.DTOs.SuperAppSecurity;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace DGC.eKYC.Business.Services.HashService;

public class HashComputeService(IConfiguration configuration, IServiceProvider serviceProvider) : IHashCompute
{
    private readonly string _superAppSecretKey = configuration.GetSection("Encryption:SuperAppHashKey").Get<string>() 
                                                 ?? throw new ArgumentException("MissingSuperAppKey");

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public bool ValidateCheckSum(string initData)
    {
        // return true if the incoming checksum matches the internally computed hash
        var lines = initData.Split('\n');

        var hashLine = lines.FirstOrDefault(l => l.StartsWith("hash=", StringComparison.OrdinalIgnoreCase));
        var providedHash = "";

        if (hashLine != null)
        {
            providedHash = hashLine.Substring("hash=".Length); // remove "hash=" prefix
        }

        var payloadForHmac = string.Join("\n", lines.Where(l => !l.StartsWith("hash=", StringComparison.OrdinalIgnoreCase)));
        var computedHash = GenerateHmac512(payloadForHmac);
        var isValidHash = string.Equals(providedHash, computedHash, StringComparison.OrdinalIgnoreCase);
        return isValidHash;
    }

    public void PopulateSecurityField(SuperAppSecurityBaseInput input, string initData)
    {
        var dict = initData
            .Split('\n')
            .Select(l => l.Split('=', 2))
            .Where(kv => kv.Length == 2)
            .ToDictionary(kv => kv[0], kv => kv[1]);

        input.CheckSum = dict.GetValueOrDefault("hash")!;
        input.DeviceId = dict.GetValueOrDefault("deviceId")!;
        input.UserLevel = dict.GetValueOrDefault("level")!;
        input.UserId = dict.GetValueOrDefault("userId")!;
        input.TimeStamp = double.Parse(dict.GetValueOrDefault("timeStamp")!);
    }

    public void ValidateSecurityFieldInput(SuperAppSecurityBaseInput input)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(input, _serviceProvider, null);

        var isValid = Validator.TryValidateObject(input, context, results, true);

        if (!isValid)
        {
            var errors = results
                .Select(ms => new ErrorDetail(string.Join(", ", ms.MemberNames), ms.ErrorMessage, []))
                .ToList();

            throw new CustomHttpResponseException(400,
                new ErrorResponse("bad_request", "Validation failed for the request.", errors));
        }
    }

    private string GenerateHmac512(string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_superAppSecretKey);
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hashBytes);
    }
}
