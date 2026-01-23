using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DGC.eKYC.Business.DTOs.DocumentType;
using DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiRequest;
using DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiResponse;
using DGC.eKYC.Business.DTOs.RedisQueueMessage;
using DGC.eKYC.Business.Services.RedisMessageQueue;
using Microsoft.Extensions.Configuration;

namespace DGC.eKYC.Business.Services.HttpCalls.HuaweiRrHttpCall;

public class HuaweiRrHttpCallService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    JsonSerializerOptions jsonSerializerOptions,
    IRedisMessageQueue<HttpCallLogQueueMessage> redisMessageQueue) : IHuaweiHttpCall
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions;
    private readonly IRedisMessageQueue<HttpCallLogQueueMessage> _redisMessageQueue = redisMessageQueue;

    public async Task<object?> ReadIdDocument(
        string ssykAccessId,
        string idBase64Image,
        EKycDocumentType documentType,
        CancellationToken cancellationToken)
    {
        object? result = null;
        var logMessage = new HttpCallLogQueueMessage();

        var ekycSection = _configuration.GetSection("HuaweiRrEKycVendor") ?? throw new ArgumentException("Missing RREkycVendor Environment Variable");
        var appName = ekycSection.GetValue<string>("AppName") ?? throw new ArgumentException("Missing RREkycVendor:AppName Environment Variable");
        var packageName = ekycSection.GetValue<string>("PackageName") ?? throw new ArgumentException("Missing RREkycVendor:PackageName Environment Variable");
        var ocrUri = ekycSection.GetValue<string>("OcrUri") ?? throw new ArgumentException("Missing RREkycVendor:OcrUri Environment Variable");

        var client = _httpClientFactory.CreateClient("HuaweiRrEKycVendor");
        var requestBody = new HuaweiRrOcrApiRequest(documentType)
        {
            AppName = appName,
            PackageName = packageName,
            ImgPath = idBase64Image,
        };

        var requestBodyStr = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, ocrUri);
        request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");

        //var logEntity = new HttpCallLog
        //{
        //    SsykAccessIdentityId = ssykAccessId,
        //    ExternalResourceId = (int)ExternalResource.HuaweiRr,
        //    RequestEndpoint = $"{client.BaseAddress}{ocrUri}",
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

        //    switch (documentType)
        //    {
        //        case EKycDocumentType.NID:
        //            var responseNidContent = await response.Content
        //                .ReadFromJsonAsync<HuaweiRrApiResponseBase<HuaweiRrKhNidOcrApiResponse>>(cancellationToken);
        //            result = responseNidContent?.Data ?? throw new InvalidOperationException("Response data from Huawei RR is null");
        //            break;
        //        case EKycDocumentType.Passport:
        //            var responsePassportContent = await response.Content
        //                .ReadFromJsonAsync<HuaweiRrApiResponseBase<HuaweiRrPassportOcrApiResponse>>(cancellationToken);
        //            result = responsePassportContent?.Data ?? throw new InvalidOperationException("Response data from Huawei RR is null");
        //            break;
        //        case EKycDocumentType.VerifyId or _:
        //            throw new ArgumentOutOfRangeException(nameof(documentType), documentType, "Document not supported");
        //    }

        //    logMessage.IsSuccessMessage = true;
        //}
        //catch (Exception)
        //{
        //    logMessage.IsSuccessMessage = true;
        //}

        //logMessage.Message = logEntity;
        await _redisMessageQueue.EnqueueAsync(logMessage, true);
        return result;
    }

    public async Task<HuaweiRrFaceCompareApiResponse?> CompareFace(
        string ssykAccessId,
        string originalFaceBase64Image,
        string newFaceBase64Image,
        CancellationToken cancellationToken)
    {
        HuaweiRrFaceCompareApiResponse? result = null;
        var logMessage = new HttpCallLogQueueMessage();

        var ekycSection = _configuration.GetSection("HuaweiRrEKycVendor") ?? throw new ArgumentException("Missing RREkycVendor Environment Variable");
        var appName = ekycSection.GetValue<string>("AppName") ?? throw new ArgumentException("Missing RREkycVendor:AppName Environment Variable");
        var packageName = ekycSection.GetValue<string>("PackageName") ?? throw new ArgumentException("Missing RREkycVendor:PackageName Environment Variable");
        var faceCompareUri = ekycSection.GetValue<string>("FaceCompareUri") ?? throw new ArgumentException("Missing RREkycVendor:FaceCompareUri Environment Variable");

        var client = _httpClientFactory.CreateClient("HuaweiRrEKycVendor");
        var requestBody = new HuaweiRrFaceCompareApiRequest
        {
            AppName = appName,
            PackageName = packageName,
            CompareImage = newFaceBase64Image,
            FaceImage = originalFaceBase64Image
        };
        var requestBodyStr = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, faceCompareUri);
        request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");

        //var logEntity = new HttpCallLog
        //{
        //    SsykAccessIdentityId = ssykAccessId,
        //    ExternalResourceId = (int)ExternalResource.HuaweiRr,
        //    RequestEndpoint = $"{client.BaseAddress}{faceCompareUri}",
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


        //    var responseContent = await response.Content.ReadFromJsonAsync<HuaweiRrApiResponseBase<HuaweiRrFaceCompareApiResponse>>(cancellationToken);
        //    result = responseContent?.Data ?? throw new InvalidOperationException("Response data from Huawei RR is null");
        //    logMessage.IsSuccessMessage = true;
        //}
        //catch (Exception)
        //{
        //    logMessage.IsSuccessMessage = true;
        //}

        //logMessage.Message = logEntity;
        await _redisMessageQueue.EnqueueAsync(logMessage, true);
        return result;
    }
}