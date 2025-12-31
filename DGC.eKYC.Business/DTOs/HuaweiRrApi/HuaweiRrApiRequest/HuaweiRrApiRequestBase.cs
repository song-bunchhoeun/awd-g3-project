namespace DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiRequest;

public class HuaweiRrApiRequestBase
{
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string Random { get; set; } = Guid.NewGuid().ToString("N");
    public string Language { get; set; } = "en";
    public string AppName { get; set; }
    public string PackageName { get; set; }
    public string Platform { get; set; } = "2";
}