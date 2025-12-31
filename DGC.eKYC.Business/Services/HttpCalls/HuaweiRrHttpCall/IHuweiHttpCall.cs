using DGC.eKYC.Business.DTOs.DocumentType;
using DGC.eKYC.Business.DTOs.HuaweiRrApi.HuaweiRrApiResponse;

namespace DGC.eKYC.Business.Services.HttpCalls.HuaweiRrHttpCall;

public interface IHuaweiHttpCall
{
    Task<object?> ReadIdDocument(
        string ssykAccessId, 
        string idBase64Image, 
        EKycDocumentType documentType,
        CancellationToken cancellationToken);

    Task<HuaweiRrFaceCompareApiResponse?> CompareFace(
        string ssykAccessId, 
        string originalFaceBase64Image, 
        string newFaceBase64Image,
        CancellationToken cancellationToken);
}