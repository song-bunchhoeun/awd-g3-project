using DGC.eKYC.Business.Services.Deeplink;
using DGC.eKYC.Business.Services.HashService;
using DGC.eKYC.Business.Services.Jwt;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace DGC.eKYC.Api.Extensions;

public static class BusinessServiceExtension
{
    public static void ConfigureBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        var serializerOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        services.AddSingleton(serializerOption);

        services.AddSingleton<IHashCompute, HashComputeService>();
        services.AddSingleton<IJwtService, ApiJwtService>();
        services.AddScoped<IDeeplinkService, DeeplinkService>();
    }
}
