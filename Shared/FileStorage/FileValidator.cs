using Shared.Exceptions;

namespace Shared.FileStorage;

public class FileValidator
{
    private readonly BlobStorageSettings _settings;

    // Magic bytes for common image formats
    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        ["image/jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        ["image/png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47 }],
        ["image/webp"] = [new byte[] { 0x52, 0x49, 0x46, 0x46 }] // RIFF header
    };

    public FileValidator(BlobStorageSettings settings)
    {
        _settings = settings;
    }

    public async Task ValidateAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        // Check content type is allowed
        if (!_settings.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                $"Content type '{contentType}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedContentTypes)}");
        }

        // Check file size
        if (stream.Length > _settings.MaxFileSizeInBytes)
        {
            var maxMb = _settings.MaxFileSizeInBytes / (1024.0 * 1024.0);
            throw new BadRequestException($"File size exceeds the maximum allowed size of {maxMb:F1}MB");
        }

        // Check magic bytes (file header)
        if (MagicBytes.TryGetValue(contentType.ToLowerInvariant(), out var signatures))
        {
            var maxLength = signatures.Max(s => s.Length);
            var header = new byte[maxLength];
            var originalPosition = stream.Position;

            stream.Position = 0;
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, maxLength), cancellationToken);
            stream.Position = originalPosition;

            if (bytesRead < maxLength)
            {
                throw new BadRequestException("File is too small to be a valid image");
            }

            var matchesAny = signatures.Any(signature =>
                header.AsSpan(0, signature.Length).SequenceEqual(signature));

            if (!matchesAny)
            {
                throw new BadRequestException(
                    $"File content does not match the declared content type '{contentType}'");
            }
        }
    }
}
