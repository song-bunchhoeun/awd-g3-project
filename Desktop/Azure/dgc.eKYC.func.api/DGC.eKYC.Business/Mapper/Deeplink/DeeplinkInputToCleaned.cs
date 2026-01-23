using DGC.eKYC.Business.DTOs.Deeplink;

namespace DGC.eKYC.Business.Mapper;

public partial class Mapper
{
    public static void ToCleaned(this GenerateDeeplinkInputDto input)
    {
        input.CallBackUrl = input.CallBackUrl.Trim();
        input.ChannelName = input.ChannelName.Trim();
        input.DealerId = input.DealerId.Trim();
        input.DealerName = input.DealerName?.Trim();
        input.PhoneNumber = input.PhoneNumber.ToMsIsdnCleaned();
    }

    public static void ToCleaned(this ValidateDeeplinkInputDto input)
    {
        input.CallBackUrl = input.CallBackUrl.Trim();
        input.DealerId = input.DealerId.Trim();
        input.PhoneNumber = input.PhoneNumber.ToMsIsdnCleaned();
        input.DeeplinkRequestId = input.DeeplinkRequestId.Trim();
    }
}