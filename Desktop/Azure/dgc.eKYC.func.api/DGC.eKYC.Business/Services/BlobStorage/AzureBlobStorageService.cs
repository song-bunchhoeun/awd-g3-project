using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using DGC.eKYC.Business.DTOs.DocumentType;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DGC.eKYC.Business.Services.BlobStorage;

public class AzureBlobStorageService(IConfiguration configuration) : IBlobStorage
{
    private readonly IConfiguration _configuration = configuration;

    // Use a dictionary to cache BlobContainerClient instances
    private readonly ConcurrentDictionary<EKycDocumentType, BlobContainerClient> _containerClients = new();

    private BlobContainerClient GetBlobContainerClient(EKycDocumentType documentType)
    {
        // Get or add the client from the cache
        return _containerClients.GetOrAdd(documentType, docType =>
        {
            var storageSettingsSection = _configuration.GetSection("Settings");
            var connectionString = _configuration.GetConnectionString("DefaultStorageConnection");

            var containerName = docType switch
            {
                EKycDocumentType.NID => storageSettingsSection.GetValue<string>("NationalIDContainer", "nid"),
                EKycDocumentType.Passport => storageSettingsSection.GetValue<string>("PassportContainer", "passport"),
                EKycDocumentType.VerifyId => storageSettingsSection.GetValue<string>("DrivingLicenseContainer", "drivinglicense"),
                _ => storageSettingsSection.GetValue<string>("ProfileContainer", "profile")
            };

            var containerClient = new BlobContainerClient(connectionString, containerName);
            return containerClient;
        });
    }

    public async Task<string> UploadFileAsync(IFormFile? file, string fileName, EKycDocumentType documentType, CancellationToken cancellationToken)
    {
        if (file == null) return string.Empty;

        var blobContainerClient = GetBlobContainerClient(documentType);
        var blobClient = blobContainerClient.GetBlobClient(fileName);
        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadBase64Async(string base64String, string fileName, EKycDocumentType documentType, CancellationToken cancellationToken)
    {
        var blobContainerClient = GetBlobContainerClient(documentType);
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        // Check if the Base64 string contains the Data URI prefix
        var dataPrefixIndex = base64String.IndexOf(',');
        var base64Data = dataPrefixIndex > -1
            ? base64String.Substring(dataPrefixIndex + 1)
            : base64String;

        var bytes = Convert.FromBase64String(base64Data);
        await using var stream = new MemoryStream(bytes);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        return blobClient.Uri.ToString();
    }

    public async Task<string> GetBase64FromBlobUriAsync(string blobUri, EKycDocumentType documentType, CancellationToken cancellationToken)
    {
        var fileUri = new Uri(blobUri);
        var blobContainerClient = GetBlobContainerClient(documentType);
        var fileName = fileUri.AbsolutePath.Replace($"{blobContainerClient.Name}/", "");
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        // Download the blob content into a BinaryData object
        var response = await blobClient.DownloadContentAsync(cancellationToken);

        // Convert the BinaryData's byte array to a Base64 string
        // The .ToArray() method gets the content as a byte array
        return Convert.ToBase64String(response.Value.Content.ToArray());
    }

    public async Task<string> GenerateSasLinkAsync(
        string blobUri, 
        EKycDocumentType documentType, 
        TimeSpan duration, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(blobUri))
        {
            return string.Empty; // Return empty if the blob URI is null or empty
        }

        var blobContainerClient = GetBlobContainerClient(documentType);
        var fileUri = new Uri(blobUri);
        var fileName = fileUri.AbsolutePath.Replace($"{blobContainerClient.Name}/", "");
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return string.Empty; 
        }

        // The BlobSasBuilder needs the container name and blob name
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobContainerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b", // "b" for blob, "c" for container
            ExpiresOn = DateTimeOffset.UtcNow.Add(duration)
        };

        // Specify permissions (e.g., Read)
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Get the SAS URI from the blob client
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public string GenerateSasLink(
        string blobUri, 
        EKycDocumentType documentType, 
        TimeSpan duration)
    {
        if (string.IsNullOrEmpty(blobUri))
        {
            return string.Empty; // Return empty if the blob URI is null or empty
        }

        var blobContainerClient = GetBlobContainerClient(documentType);
        var fileUri = new Uri(blobUri);
        var fileName = fileUri.AbsolutePath.Replace($"{blobContainerClient.Name}/", "");
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        if (!blobClient.Exists())
        {
            return string.Empty; 
        }

        // The BlobSasBuilder needs the container name and blob name
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobContainerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b", // "b" for blob, "c" for container
            ExpiresOn = DateTimeOffset.UtcNow.Add(duration)
        };

        // Specify permissions (e.g., Read)
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Get the SAS URI from the blob client
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task<string> DownloadFileBase64Async(string blobUri, EKycDocumentType documentType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(blobUri))
            return string.Empty;

        var fileUri = new Uri(blobUri);
        var blobContainerClient = GetBlobContainerClient(documentType);
        var fileName = fileUri.AbsolutePath.Replace($"{blobContainerClient.Name}/", "");
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return string.Empty;

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        var base64ImageString = Convert.ToBase64String(response.Value.Content);
        return base64ImageString;
    }
}