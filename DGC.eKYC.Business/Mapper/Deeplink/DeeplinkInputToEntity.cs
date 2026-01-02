using DGC.eKYC.Business.DTOs.Deeplink;
using DGC.eKYC.Dal.Models;

namespace DGC.eKYC.Business.Mapper;

public partial class Mapper
{
    public static DeepLinkRequest ToDeepLinkRequestEntity(this GenerateDeeplinkInputDto input, Guid id, int orgId, DateTime now)
    {
        var deeplinkEntity = new DeepLinkRequest
        {
            Id = id,
            OrgId = orgId,
            DealerId = input.DealerId,
            DealerName = input.DealerName,
            PhoneNumber = input.PhoneNumber,
            CallBackUrl = input.CallBackUrl,
            ChannelName = input.ChannelName,
            CreatedAt = now,
            IsUsed = false
        };

        return deeplinkEntity;
    }

    public static OrgChannel ToOrgChannelEntity(this GenerateDeeplinkInputDto input, int orgId, DateTime now)
    {
        var deeplinkEntity = new OrgChannel
        {
            OrgId = orgId,
            ChannelName = input.ChannelName,
            CreatedDate = now,
        };

        return deeplinkEntity;
    }
}