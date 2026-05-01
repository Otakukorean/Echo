namespace Shared.Contracts.FileStorage;

public record FileUploadResult(string Url, string BlobName);

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string containerName,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default);
}
