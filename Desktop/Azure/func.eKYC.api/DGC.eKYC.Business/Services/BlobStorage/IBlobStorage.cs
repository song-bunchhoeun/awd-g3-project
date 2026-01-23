using DGC.eKYC.Business.DTOs.DocumentType;
using Microsoft.AspNetCore.Http;

namespace DGC.eKYC.Business.Services.BlobStorage;

public interface IBlobStorage
{
    Task<string> UploadFileAsync(IFormFile? file, string fileName, EKycDocumentType documentType, CancellationToken cancellationToken);
    Task<string> UploadBase64Async(string base64String, string fileName, EKycDocumentType documentType, CancellationToken cancellationToken);
    Task<string> GetBase64FromBlobUriAsync(string blobUri, EKycDocumentType documentType, CancellationToken cancellationToken);
    Task<string> GenerateSasLinkAsync(string blobUri, EKycDocumentType documentType, TimeSpan duration, CancellationToken cancellationToken);
    Task<string> DownloadFileBase64Async(string blobUri, EKycDocumentType documentType, CancellationToken cancellationToken);

    string GenerateSasLink(
        string blobUri,
        EKycDocumentType documentType,
        TimeSpan duration);
}