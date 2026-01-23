using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DGC.eKYC.Business.DTOs.RedisQueueMessage;
using DGC.eKYC.Business.Services.HttpCalls.PotApi;
using DGC.eKYC.Business.Services.HttpCalls.PotApi.VerifySimProfile;
using DGC.eKYC.Business.Services.RedisMessageQueue;
using Microsoft.Extensions.Configuration;

namespace DGC.eKYC.Business.Services.HttpCalls.PointOfTruthHttpCall;

public class PointOfTruthHttpCallService(
    IHttpClientFactory httpClientFactory, 
    IConfiguration configuration,
    IRedisMessageQueue<HttpCallLogQueueMessage> redisMessageQueue,
    JsonSerializerOptions jsonSerializerOptions) : IPointOfTruthHttpCall
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly IRedisMessageQueue<HttpCallLogQueueMessage> _redisMessageQueue = redisMessageQueue;
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions;

    public async Task<PotApiVerifySimProfileOutputDto?> VerifySimProfileAsync(
        string ssykAccessId, 
        PotApiVerifySimProfileInputPersonDocumentDto inputDto,
        CancellationToken cancellationToken)
    {
        PotApiVerifySimProfileOutputDto? result = null;
        var logMessage = new HttpCallLogQueueMessage();

        var verifySimProfileUri = _configuration.GetValue<string>("PotApi:VerifySimProfileEndpoint", "ssyk/verify-sim-profile?api-version=2025-06-14");
        var client = _httpClientFactory.CreateClient("PointOfTruthApi");
        var request = new HttpRequestMessage(HttpMethod.Post, verifySimProfileUri);
        var requestBodyStr = JsonSerializer.Serialize(inputDto, _jsonSerializerOptions);
        request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");
        //var logEntity = new HttpCallLog
        //{
        //    SsykAccessIdentityId = ssykAccessId,
        //    ExternalResourceId = (int)ExternalResource.PointOfTruth,
        //    RequestEndpoint = $"{client.BaseAddress}{verifySimProfileUri}",
        //    RequestBodyJson = requestBodyStr,
        //    ResponseStatusCode = 0, // Will be updated after the call
        //    ResponseBodyJson = string.Empty, // Will be updated after the call
        //    ResponseTimeMs = 0, // Will be updated after the call
        //    CreatedDate = DateTime.UtcNow
        //};

        //var stopWatch = new Stopwatch();
        //stopWatch.Start();

        //try
        //{
        //    var response = await client.SendAsync(request, cancellationToken);
        //    stopWatch.Stop();
        //    logEntity.ResponseStatusCode = (int)response.StatusCode;
        //    logEntity.ResponseBodyJson = await response.Content.ReadAsStringAsync(cancellationToken);
        //    logEntity.ResponseTimeMs = (int)stopWatch.ElapsedMilliseconds;
        //    response.EnsureSuccessStatusCode();

        //    var responseContent = await response.Content.ReadFromJsonAsync<PotApiBaseResponse<PotApiVerifySimProfileOutputDto>>(cancellationToken);
        //    logMessage.IsSuccessMessage = true;
        //    result = responseContent?.Data ?? throw new InvalidOperationException("Response data from POT is null");
        //}
        //catch (Exception)
        //{
        //    logMessage.IsSuccessMessage = false;
        //}

        //logMessage.Message = logEntity;
        await _redisMessageQueue.EnqueueAsync(logMessage, true);
        return result;
    }
}