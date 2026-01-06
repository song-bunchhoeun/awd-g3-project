using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Deeplink;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.Mapper;
using DGC.eKYC.Business.Services.CustomHybridCache;
using DGC.eKYC.Business.Services.Jwt;
using DGC.eKYC.Dal.Contexts;
using DGC.eKYC.Dal.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;

namespace DGC.eKYC.Business.Services.Deeplink;

public class DeeplinkService(
    EKycContext eKycContext,
    IConfiguration configuration,
    IJwtService jwtService,
    HybridCache hybridCache) : IDeeplinkService
{
    private readonly EKycContext _eKycContext = eKycContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<GenerateDeeplinkOutputDto> GenerateDeeplink(
        GenerateDeeplinkInputDto generateDeeplinkInputInputDto,
        string mnoDgConnectClientId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var deeplinkId = Guid.NewGuid();
        var deeplinkIdStr = deeplinkId.ToString();
        var timestamp = now.ToUnixTimeSeconds();

        var entityKey = new object[] { mnoDgConnectClientId };
        var clientEntity = await _eKycContext.OrgDgconnectClients.FindAsync(entityKey, cancellationToken);
        if (clientEntity == null)
            throw new CustomHttpResponseException(
                403,
                new ErrorResponse("forbidden_client_id", "client id is not valid", []));

        if (clientEntity.DeletedAt != null)
            throw new CustomHttpResponseException(
                403,
                new ErrorResponse("forbidden_client_id", "client id is no longer valid", []));

        _eKycContext.Entry(clientEntity).State = EntityState.Detached;

        var superAppRedirectUrl = _configuration.GetValue<string?>("SuperAppSettings:MobileRedirectUrl")
                                  ?? throw new ArgumentNullException(nameof(configuration), "missing SuperApp mobile redirect url");

        var eKycMiniAppId = _configuration.GetValue<string?>("SuperAppSettings:EKycMiniAppId")
                            ?? throw new ArgumentNullException(nameof(configuration), "missing eKyc miniapp id");

        var miniAppActionName = _configuration.GetValue<string?>("SuperAppSettings:MiniAppActionName", "action")
                            ?? throw new ArgumentNullException(nameof(configuration), "missing eKyc miniapp action name");

        var miniAppHost = _configuration.GetValue<string?>("SuperAppSettings:MiniAppHost");

        var queryParams = generateDeeplinkInputInputDto.ToParamDictionary(
            eKycMiniAppId,
            timestamp.ToString(),
            deeplinkIdStr,
            clientEntity.OrgId,
            miniAppHost,
            miniAppActionName);

        var callbackUrl = QueryHelpers.AddQueryString(superAppRedirectUrl, queryParams);
        var deeplinkExpiration = _configuration.GetValue<int>("EkycSettings:DeeplinkExpirationSeconds");

        var redisTask = InsertDeeplinkRequestToRedis(generateDeeplinkInputInputDto, deeplinkIdStr, timestamp, clientEntity, deeplinkExpiration, cancellationToken);
        var dbTask = InsertDeeplinkRequestToDb(generateDeeplinkInputInputDto, deeplinkId, clientEntity, now, cancellationToken);

        var allTask = Task.WhenAll(redisTask, dbTask);
        var response = new GenerateDeeplinkOutputDto(callbackUrl, deeplinkExpiration);
        await allTask;
        return response;
    }

    public async Task<ValidateDeeplinkOutputDto> ValidateDeeplink(
        ValidateDeeplinkInputDto validateDeeplinkDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheDto = await _hybridCache.GetAsync<DeeplinkCacheDto?>(validateDeeplinkDto.DeeplinkRequestId, cancellationToken);
            if (cacheDto == null)
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_request", "this request is not valid!"));

            if (!string.Equals(validateDeeplinkDto.CallBackUrl, cacheDto.CallBackUrl, StringComparison.InvariantCultureIgnoreCase))
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_callback", "this callbackUrl is not valid!"));

            if (!string.Equals(validateDeeplinkDto.DealerId, cacheDto.DealerId, StringComparison.InvariantCultureIgnoreCase))
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_dealerId", "this DealerId is not valid!"));

            if (!string.Equals(validateDeeplinkDto.PhoneNumber, cacheDto.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_phone_number", "this PhoneNumber is not valid!"));

            if (validateDeeplinkDto.OrgId != cacheDto.OrgId)
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_org_id", "this orgId is not valid!"));

            if (validateDeeplinkDto.Timestamp != cacheDto.Timestamp)
                throw new CustomHttpResponseException(403, new ErrorResponse("forbidden_timestamp", "this Timestamp is not valid!"));

            var deeplinkExpiration = _configuration.GetValue<int>("EKycSettings:DeeplinkExpirationSeconds");
            var originalTimestamp = DateTimeOffset.FromUnixTimeSeconds(cacheDto.Timestamp);
            var nowTime = DateTimeOffset.UtcNow;
            var elapsedSeconds = (nowTime - originalTimestamp).TotalSeconds;
            if (elapsedSeconds > deeplinkExpiration)
                throw new CustomHttpResponseException(410, new ErrorResponse("expired_link", "this deeplink is expired!"));

            var eKycTransactionId = Guid.NewGuid();
            await InsertTransactionToDb(validateDeeplinkDto.DeeplinkRequestId, validateDeeplinkDto, nowTime.DateTime, eKycTransactionId, cancellationToken);
            await InvalidateDeeplinkCache(validateDeeplinkDto.DeeplinkRequestId);

            var eKycTransactionExpiration = _configuration.GetValue<int>("EKycSettings:TransactionExpirationSeconds");
            var jwtParams = new Dictionary<string, string?>
            {
                { "dealerId", cacheDto.DealerId },
                { "orgId", cacheDto.OrgId.ToString() },
                { "eKycTransactionId", eKycTransactionId.ToString() }
            };
            var response = _jwtService.GenerateToken(eKycTransactionId.ToString(), eKycTransactionExpiration, nowTime, jwtParams);
            return new ValidateDeeplinkOutputDto(response, eKycTransactionExpiration, nowTime.Date);
        }
        catch (CustomHttpResponseException ex)
        {
            //TODO: Log Action
            Console.WriteLine($"{ex.Value}");
            throw;
        }
    }

    private async Task InsertTransactionToDb(string deeplinkIdStr, ValidateDeeplinkInputDto apiInputDto, DateTime now, Guid id, CancellationToken cancellationToken)
    {
        var deeplinkEntityKey = new object[] { Guid.Parse(deeplinkIdStr) };
        var deeplinkEntity = await _eKycContext.DeepLinkRequests.FindAsync(deeplinkEntityKey, cancellationToken);
        if (deeplinkEntity == null)
            throw new CustomHttpResponseException(500, new ErrorResponse("something_went_wrong", "something went wrong with Deeplink DB!"));

        deeplinkEntity.IsUsed = true;
        deeplinkEntity.UsedAt = now;
        deeplinkEntity.ClientIpAddress = apiInputDto.IpAddress;
        deeplinkEntity.UserAgent = apiInputDto.UserAgent;

        deeplinkEntity.EkycTransaction = deeplinkEntity.ToEkycTransactionEntity(apiInputDto, null, id, now);
        await _eKycContext.SaveChangesAsync(cancellationToken);
    }

    private async Task InvalidateDeeplinkCache(string deeplinkIdStr)
    {
        try
        {
            await _hybridCache.RemoveByTagAsync(deeplinkIdStr);
            await _hybridCache.RemoveAsync(deeplinkIdStr);
        }
        catch
        {
            // Ignore
        }

    }

    private async Task InsertDeeplinkRequestToDb(
        GenerateDeeplinkInputDto generateDeeplinkInputInputDto,
        Guid deeplinkId,
        OrgDgconnectClient clientEntity,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var deeplinkEntity = generateDeeplinkInputInputDto.ToDeepLinkRequestEntity(deeplinkId, clientEntity.OrgId, now.DateTime);

        var orgChannelEntityKey = new object[] { generateDeeplinkInputInputDto.ChannelName, clientEntity.OrgId };
        var orgChannelEntity = await _eKycContext.OrgChannels.FindAsync(orgChannelEntityKey, cancellationToken);
        if (orgChannelEntity == null)
        {
            orgChannelEntity = generateDeeplinkInputInputDto.ToOrgChannelEntity(clientEntity.OrgId, now.DateTime);
            await _eKycContext.OrgChannels.AddAsync(orgChannelEntity, cancellationToken);
        }

        await _eKycContext.DeepLinkRequests.AddAsync(deeplinkEntity, cancellationToken);
        await _eKycContext.SaveChangesAsync(cancellationToken);
    }

    private async Task InsertDeeplinkRequestToRedis(
        GenerateDeeplinkInputDto generateDeeplinkInputInputDto,
        string deeplinkIdStr,
        long timestamp,
        OrgDgconnectClient clientEntity,
        int deeplinkExpiration,
        CancellationToken cancellationToken)
    {
        var cacheDto =
            generateDeeplinkInputInputDto.ToCache(generateDeeplinkInputInputDto.CallBackUrl, deeplinkIdStr, timestamp, clientEntity.OrgId);

        await _hybridCache.CreateCacheAsync(deeplinkIdStr,
            cacheDto,
            deeplinkExpiration,
            deeplinkIdStr,
            cancellationToken);
    }
}