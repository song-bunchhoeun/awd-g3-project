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


        //services.AddSingleton<IHashCompute, HashComputeService>();
        //services.AddSingleton<AcledaOperatorInfoStore>();
        //services.AddSingleton<ICryptoUtils, CryptoUtils>();
        //services.AddSingleton<IAcledaApiCall, AcledaApiCallService>();
        //services.AddTransient<IAcledaTopUp, AcledaTopUpService>();
        //services.AddSingleton<IRedisMessageQueue<TopUpTransaction>>(provider =>
        //{
        //    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
        //    var queueName = configuration.GetValue<string>("RedisQueue:TopUpTransactionQueue", "top-up-transaction-queue");
        //    return new RedisMessageQueueService<TopUpTransaction>(redis, queueName);
        //});

        //services.AddSingleton<IRedisMessageQueue<GuestUser>>(provider =>
        //{
        //    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
        //    var queueName = configuration.GetValue<string>("RedisQueue:UserOperationQueue", "user-operation-queue");
        //    return new RedisMessageQueueService<GuestUser>(redis, queueName);
        //});

        //services.AddSingleton<IRedisMessageQueue<DetailPartnerApiCall>>(provider =>
        //{
        //    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
        //    var queueName = configuration.GetValue<string>("RedisQueue:DetailPartnerErrorQueue", "log-partner-api-queue");
        //    return new RedisMessageQueueService<DetailPartnerApiCall>(redis, queueName);
        //});

        //services.AddHostedService<TransactionService>();
        //services.AddHostedService<LogPartnerApiCallErrorService>();
        //services.AddHostedService<UserService>();
        //services.AddHostedService<AcledaOperatorInfoBackgroundService>();
    }
}
