using System.Text;
using System.Text.Json;
using DGC.eKYC.Business.DTOs.RedisQueueMessage;
using DGC.eKYC.Business.Services.RedisMessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DGC.eKYC.Business.Services.HttpCalls;

public class HttpLogBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<HttpLogBackgroundService> logger)
    : BaseBackgroundService<HttpCallLogQueueMessage>(serviceScopeFactory,logger)
{
    private readonly ILogger<HttpLogBackgroundService> _logger = logger;

    protected override async Task ProcessMessageAsync(IServiceProvider serviceProvider, HttpCallLogQueueMessage message, CancellationToken token)
    {
        //try
        //{
        //    _logger.LogInformation(
        //        "Starting Http Call Log Queue for {0} to {1} at {2}",
        //        message.Message.SsykAccessIdentityId,
        //        message.Message.ExternalResourceId,
        //        message.Message.RequestEndpoint);

        //    var dbContext = serviceProvider.GetRequiredService<SimRegistrationContext>();
        //    await dbContext.HttpCallLogs.AddAsync(message.Message, token);
        //    await dbContext.SaveChangesAsync(token);

        //    if (!message.IsSuccessMessage)
        //    {
        //        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        //        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        //        var jsonSerializerOptions = serviceProvider.GetRequiredService<JsonSerializerOptions>();
        //        var httpClient = httpClientFactory.CreateClient("FlowLogger");
        //        var exceptionEndpoint = configuration.GetValue<string?>("Logger:HttpCallLog");
        //        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, exceptionEndpoint);
        //        message.Message.CreatedDate = message.Message.CreatedDate.AddHours(7); // Use Kh Time
        //        var requestBodyJson = JsonSerializer.Serialize(message.Message, jsonSerializerOptions);
        //        httpRequestMessage.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
        //        await httpClient.SendAsync(httpRequestMessage, token);
        //    }

        //    _logger.LogInformation(
        //        "Successfully Processed Http Call Log Queue for {0} to {1}",
        //        message.Message.SsykAccessIdentityId,
        //        message.Message.ExternalResourceId);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogWarning(
        //        "Log Http Call Failed: {HttpCallLogQueueMessage} at {DateTime} because {Exception}", 
        //        message, 
        //        DateTime.UtcNow, 
        //        ex);

        //    throw;
        //}
    }
}