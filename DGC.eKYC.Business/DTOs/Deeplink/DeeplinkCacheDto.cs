namespace DGC.eKYC.Business.DTOs.Deeplink;

public class DeeplinkCacheDto
{
    public string DeeplinkRequestId { get; set; }
    public string DealerId { get; set; }
    public string PhoneNumber { get; set; }
    public long Timestamp { get; set; }
    public string CallBackUrl { get; set; }
    public int OrgId { get; set; }
}