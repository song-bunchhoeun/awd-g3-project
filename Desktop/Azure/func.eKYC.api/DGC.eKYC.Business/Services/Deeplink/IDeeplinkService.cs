using DGC.eKYC.Business.DTOs.Deeplink;

namespace DGC.eKYC.Business.Services.Deeplink;

public interface IDeeplinkService
{
    Task<GenerateDeeplinkOutputDto> GenerateDeeplink(
        GenerateDeeplinkInputDto generateDeeplinkInputInputDto, 
        string mnoDgConnectClientId, 
        CancellationToken cancellationToken);

    Task<ValidateDeeplinkOutputDto> ValidateDeeplink(
        ValidateDeeplinkInputDto validateDeeplinkDto,
        CancellationToken cancellationToken);
}