namespace DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiRequest;

public class HuaweiRrFaceCompareApiRequest: HuaweiRrApiRequestBase
{
    public string ReqFlag { get; set; } = "2";
    public string FaceImage { get; set; }
    public string CompareImage { get; set; }
}