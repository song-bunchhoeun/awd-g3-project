using DGC.eKYC.Deeplink.Attributes;
using DGC.eKYC.Deeplink.Functions.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DGC.eKYC.Deeplink.Functions;

[ApiVersion("2025-05-09")] 
public class DeeplinkController(ILogger<DeeplinkController> logger, IConfiguration configuration) : BaseFunction(configuration)
{
    private readonly ILogger<DeeplinkController> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    //[Function("api/deeplink")]
    //public async Task<IActionResult> CreateDeeplink([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    //{
    //    _logger.LogInformation("C# HTTP trigger function processed a request.");

    //    var testObj = new
    //    {
    //        test =  "test",
    //        abc = "abc"
    //    };

    //    await Task.CompletedTask;

    //    return new OkObjectResult(testObj);
    //}

    [Function("deeplink")]
    public async Task<IActionResult> CreateDeeplink([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        return await ExecuteAsync<object>(req, ProcessDeeplinkRequestAsync);
    }

    private async Task<IActionResult> ProcessDeeplinkRequestAsync(object model)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var testObj = new
        {
            test = _configuration.GetValue<string>("PonereaySecret1:Secret"),
            abc = "abc",
            received = model
        };

        await Task.CompletedTask;
        return new OkObjectResult(testObj);
    }
}