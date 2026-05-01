using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Shared.Contracts.FileStorage;

namespace Shared.FileStorage;

public class BlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly FileValidator _fileValidator;

    public BlobStorageService(BlobServiceClient blobServiceClient, FileValidator fileValidator)
    {
        _blobServiceClient = blobServiceClient;
        _fileValidator = fileValidator;
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string containerName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        await _fileValidator.ValidateAsync(stream, contentType, cancellationToken);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".bin";
        var blobName = string.IsNullOrWhiteSpace(folder)
            ? $"{Guid.NewGuid()}{extension}"
            : $"{folder.TrimEnd('/')}/{Guid.NewGuid()}{extension}";

        var blobClient = containerClient.GetBlobClient(blobName);

        stream.Position = 0;

        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);

        return new FileUploadResult(blobClient.Uri.ToString(), blobName);
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(blobUrl);

        // URL format: https://{account}.blob.core.windows.net/{container}/{blobName}
        var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
        if (segments.Length < 2) return;

        var containerName = segments[0];
        var blobName = segments[1];

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
