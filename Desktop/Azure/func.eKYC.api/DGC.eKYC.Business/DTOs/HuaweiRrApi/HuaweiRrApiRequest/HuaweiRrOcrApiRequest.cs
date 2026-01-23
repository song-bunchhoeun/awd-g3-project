using DGC.eKYC.Business.DTOs.DocumentType;

namespace DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiRequest;

public class HuaweiRrOcrApiRequest(EKycDocumentType docType) : HuaweiRrApiRequestBase
{
    public string ReqFlag { get; set; } = "3";
    public string Kind { get; set; } = docType switch
    {
        EKycDocumentType.NID => "1",
        EKycDocumentType.Passport => "0",
        EKycDocumentType.VerifyId or _=> 
            throw new ArgumentOutOfRangeException($"Unsupported Document Type {docType} for EKYC Provider"),
    };
    public string ImgPath { get; set; }
}