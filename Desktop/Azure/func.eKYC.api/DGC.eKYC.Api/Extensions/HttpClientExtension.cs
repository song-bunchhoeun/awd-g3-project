using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Wrap;

namespace DGC.eKYC.Api.Extensions;

public static class HttpClientExtension
{
    public static void ConfigureHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        //POT
        var potSection = configuration.GetSection("PotApi");
        services.AddHttpClient("PointOfTruthApi", httpClient =>
        {
            var baseAddress = potSection.GetValue<string>("BaseUri")
                              ?? throw new ArgumentNullException("missing pot baseurl");
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json");
        })
        .AddPolicyHandler(BuildPolicy(potSection));

        // FlowLogger
        var loggerSection = configuration.GetSection("Logger");
        services.AddHttpClient("FlowLogger", httpClient =>
        {
            var baseAddress = loggerSection.GetValue<string>("BaseUri")
                              ?? throw new ArgumentNullException("missing logger baseurl");
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        })
        .AddPolicyHandler(BuildPolicy(loggerSection));

        // HuaweiRrEKycVendor
        var huaweiSection = configuration.GetSection("HuaweiRrEKycVendor");
        services.AddHttpClient("HuaweiRrEKycVendor", httpClient =>
        {
            var baseAddress = huaweiSection.GetValue<string>("Uri")
                              ?? throw new ArgumentNullException("missing huawei RR baseurl");
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        })
        .AddPolicyHandler(BuildPolicy(huaweiSection));
    }

    private static AsyncPolicyWrap<HttpResponseMessage> BuildPolicy(IConfigurationSection section)
    {
        // Configuration values with sensible defaults
        var retryCount = section.GetValue("RetryCount", 3);
        var baseDelaySeconds = section.GetValue("RetryDelaySeconds", 1);
        var jitterMs = section.GetValue("RetryJitterMilliseconds", 500);
        var circuitBreakerFailures = section.GetValue("CircuitBreakerFailures", 3);
        var circuitBreakerDuration = TimeSpan.FromSeconds(section.GetValue("CircuitBreakerDurationSeconds", 30));

        var random = new Random();

        // Exponential backoff with jitter
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(baseDelaySeconds, retryAttempt)) +
                    TimeSpan.FromMilliseconds(random.Next(0, jitterMs)),
                onRetry: async void (outcome, timespan, retryAttempt, _) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString();
                    var error = outcome.Exception?.Message ?? statusCode;
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds:F2}s due to {error}");
                    await Task.CompletedTask;
                }
            );

        // Advanced circuit breaker with logging
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: circuitBreakerFailures,
                durationOfBreak: circuitBreakerDuration,
                onBreak: async void (outcome, breakDelay) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString();
                    var error = outcome.Exception?.Message ?? statusCode;
                    Console.WriteLine($"Circuit broken for {breakDelay.TotalSeconds:F2}s due to {error}");
                    await Task.CompletedTask;
                },
                onReset: () => Console.WriteLine("Circuit reset."),
                onHalfOpen: () => Console.WriteLine("Circuit half-open.")
            );

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}
