using DGC.eKYC.Business.DTOs.CustomExceptions;
using DGC.eKYC.Business.DTOs.Deeplink;
using DGC.eKYC.Business.DTOs.Errors;
using DGC.eKYC.Business.Mapper;
using DGC.eKYC.Business.Services.CustomHybridCache;
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
    HybridCache hybridCache) : IDeeplinkService
{
    private readonly EKycContext _eKycContext = eKycContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly HybridCache _hybridCache = hybridCache;

    public async Task<GenerateDeeplinkOutputDto> GenerateDeeplink(
        GenerateDeeplinkInputDto generateDeeplinkInputDto,
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

        var superAppRedirectUrl = _configuration.GetValue<string?>("SuperApp:MobileRedirectUrl") 
                                  ?? throw new ArgumentNullException(nameof(configuration), "missing SuperApp mobile redirect url");

        var eKycMiniAppId = _configuration.GetValue<string?>("SuperApp:EKycMiniAppId") 
                            ?? throw new ArgumentNullException(nameof(configuration), "missing eKyc miniapp id");

        var queryParams = generateDeeplinkInputDto.ToParamDictionary(eKycMiniAppId, timestamp.ToString(), clientEntity.OrgId);
        var callbackUrl = QueryHelpers.AddQueryString(superAppRedirectUrl, queryParams);
        var deeplinkExpiration = _configuration.GetValue<int>("EKycDeeplink:ExpirationSeconds");

        var redisTask = InsertDeeplinkRequestToRedis(generateDeeplinkInputDto, deeplinkIdStr, callbackUrl, timestamp, clientEntity, deeplinkExpiration, cancellationToken);
        var dbTask = InsertDeeplinkRequestToDb(generateDeeplinkInputDto, deeplinkId, clientEntity, now, cancellationToken);

        var allTask = Task.WhenAll(redisTask, dbTask);
        var response = new GenerateDeeplinkOutputDto(callbackUrl, deeplinkExpiration);
        await allTask;
        return response;
    }

    private async Task InsertDeeplinkRequestToDb(
        GenerateDeeplinkInputDto generateDeeplinkInputDto,
        Guid deeplinkId,
        OrgDgconnectClient clientEntity,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var deeplinkEntity = generateDeeplinkInputDto.ToDeepLinkRequestEntity(deeplinkId, clientEntity.OrgId, now.DateTime);

        var orgChannelEntityKey = new object[] { generateDeeplinkInputDto.ChannelName, clientEntity.OrgId };
        var orgChannelEntity = await _eKycContext.OrgChannels.FindAsync(orgChannelEntityKey, cancellationToken);
        if (orgChannelEntity == null)
        {
            orgChannelEntity = generateDeeplinkInputDto.ToOrgChannelEntity(clientEntity.OrgId, now.DateTime);
            await _eKycContext.OrgChannels.AddAsync(orgChannelEntity, cancellationToken);
        }

        await _eKycContext.DeepLinkRequests.AddAsync(deeplinkEntity, cancellationToken);
        await _eKycContext.SaveChangesAsync(cancellationToken);
    }

    private async Task InsertDeeplinkRequestToRedis(
        GenerateDeeplinkInputDto generateDeeplinkInputDto,
        string deeplinkIdStr,
        string callbackUrl, 
        long timestamp, 
        OrgDgconnectClient clientEntity, 
        int deeplinkExpiration,
        CancellationToken cancellationToken)
    {
        var cacheDto =
            generateDeeplinkInputDto.ToCache(callbackUrl, deeplinkIdStr, timestamp, clientEntity.OrgId);

        await _hybridCache.CreateCacheAsync(deeplinkIdStr,
            cacheDto,
            deeplinkExpiration,
            deeplinkIdStr,
            cancellationToken);
    }
}