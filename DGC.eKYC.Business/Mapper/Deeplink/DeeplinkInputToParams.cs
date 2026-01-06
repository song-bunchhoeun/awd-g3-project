using DGC.eKYC.Business.DTOs.Deeplink;

namespace DGC.eKYC.Business.Mapper;

public partial class Mapper
{
    public static Dictionary<string, string?> ToParamDictionary(
        this GenerateDeeplinkInputDto input,
        string eKycMiniAppId,
        string unixTimestampStr,
        string deeplinkRequestId,
        int orgId,
        string? host,
        string actionName)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "name", actionName },
            { "host", host },
            { "miniappid", eKycMiniAppId },
            { "timestamp", unixTimestampStr },
            { "callbackUrl", input.CallBackUrl },
            { "dealerId", input.DealerId },
            { "orgId", orgId.ToString() },
            { "phoneNumber", input.PhoneNumber },
            { "channelName", input.ChannelName },
            { "deeplinkId", deeplinkRequestId}
        };

        return queryParams;
    }
}