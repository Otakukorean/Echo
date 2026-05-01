namespace Shared.FileStorage;

public class BlobStorageSettings
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; init; } = string.Empty;
    public long MaxFileSizeInBytes { get; init; } = 5 * 1024 * 1024; // 5MB default
    public string[] AllowedContentTypes { get; init; } = ["image/jpeg", "image/png", "image/webp"];
}
