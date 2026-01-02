using DGC.eKYC.Business.DTOs.Deeplink;

namespace DGC.eKYC.Business.Mapper;

public partial class Mapper
{
    public static DeeplinkCacheDto ToCache(
        this GenerateDeeplinkInputDto input, 
        string callBackUrl,
        string deeplinkRequestId,
        long timestampLong,
        int orgId)
    {
        var cacheDto = new DeeplinkCacheDto
        {
            CallBackUrl = callBackUrl,
            DealerId = input.DealerId,
            DeeplinkRequestId = deeplinkRequestId,
            OrgId = orgId,
            PhoneNumber = input.PhoneNumber,
            Timestamp = timestampLong
        };

        return cacheDto;
    }
}