namespace DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiResponse;

public class HuaweiRrApiResponseBase<T>
{
    public string Status { get; set; }
    public List<string> MessageVariable { get; set; }
    public string Message { get; set; }
    public int Code { get; set; }
    public T Data { get; set; }
}