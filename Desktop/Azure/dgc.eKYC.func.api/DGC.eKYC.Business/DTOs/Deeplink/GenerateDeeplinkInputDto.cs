using System.ComponentModel.DataAnnotations;
using DGC.eKYC.Business.Validations.ModelState;

namespace DGC.eKYC.Business.DTOs.Deeplink;

public class GenerateDeeplinkInputDto
{
    [StringLength(64, ErrorMessage = "Dealer Id cannot be longer than 64 characters. Please contact administrator!")]
    public string DealerId { get; set; }

    [StringLength(128, ErrorMessage = "Dealer Name cannot be longer than 256 characters. Please contact administrator!")]
    public string? DealerName { get; set; }

    [CambodianPhoneNumberValidation]
    public string PhoneNumber { get; set; }

    [CallBackUrlValidation]
    public string CallBackUrl { get; set; }

    [StringLength(64, ErrorMessage = "Channel Name cannot be longer than 64 characters. Please contact administrator!")]
    public string ChannelName { get; set; }
}

public class GenerateDeeplinkOutputDto(string deeplinkUrl, int expiredInSeconds)
{
    public string DeeplinkUrl { get; set; } = deeplinkUrl;
    public int ExpiresIn { get; set; } = expiredInSeconds;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddSeconds(expiredInSeconds);
}