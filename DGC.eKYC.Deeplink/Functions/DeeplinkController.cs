using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Deeplink;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.Mapper;
using DGC.eKYC.Business.Services.Deeplink;
using DGC.eKYC.Business.Services.Jwt;
using DGC.eKYC.Deeplink.Attributes;
using DGC.eKYC.Deeplink.Functions.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DGC.eKYC.Deeplink.Functions;

[ApiVersion("2025-11-20")]
public class DeeplinkController(
    ILogger<DeeplinkController> logger,
    IConfiguration configuration, 
    IJwtService jwtService,
    IDeeplinkService deeplinkService) : BaseFunction(configuration)
{
    private readonly ILogger<DeeplinkController> _logger = logger;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IDeeplinkService _deeplinkService = deeplinkService;

    [Function("generate-deeplink")]
    public async Task<IActionResult> CreateDeeplink(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deeplink/generate-mno")] HttpRequest req, 
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync<GenerateDeeplinkInputDto>(req, ProcessDeeplinkRequestAsync, cancellationToken);
    }

    private async Task<IActionResult> ProcessDeeplinkRequestAsync(
        HttpRequest req, 
        GenerateDeeplinkInputDto generateDeeplinkInputDto, 
        CancellationToken cancellationToken)
    {
        generateDeeplinkInputDto.ToCleaned();

        var authTokenAvailable = req.Headers.TryGetValue("Authorization", out var authToken);
        if (!authTokenAvailable)
            throw new CustomHttpResponseException(
                400, 
                new ErrorResponse("missing_auth_token", "Please attach Authorization Token to Header!"));

        var mnoDgConnectClientId = _jwtService.GetClaimValue<string>(authToken.ToString(), "sub");
        if (string.IsNullOrEmpty(mnoDgConnectClientId))
            throw new CustomHttpResponseException(403,
                new ErrorResponse("malformed_token", "Authorize Token is malformed!"));

        var response = 
            await _deeplinkService.GenerateDeeplink(generateDeeplinkInputDto, mnoDgConnectClientId, cancellationToken);

        return new OkObjectResult(response);
    }
}