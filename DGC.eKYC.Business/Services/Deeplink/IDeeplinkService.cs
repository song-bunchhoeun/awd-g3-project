using DGC.eKYC.Business.DTOs.Deeplink;

namespace DGC.eKYC.Business.Services.Deeplink;

public interface IDeeplinkService
{
    Task<GenerateDeeplinkOutputDto> GenerateDeeplink(
        GenerateDeeplinkInputDto generateDeeplinkInputDto, 
        string mnoDgConnectClientId, 
        CancellationToken cancellationToken);


}