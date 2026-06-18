using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Alco.Functions.Services;

public sealed class BlobFileNameResolver(BlobServiceClientFactory blobServiceClientFactory, IConfiguration configuration)
{
    private static readonly string[] DirectFileNameKeys = ["fileName", "filename", "metadata_storage_name", "metadataStorageName", "blobName"];
    private static readonly string[] FilePathKeys = ["metadata_storage_path", "metadataStoragePath", "blobPath", "path", "blobUrl", "url", "documentUrl"];
    private static readonly string[] ContainerKeys = ["container", "containerName", "metadata_storage_container", "metadataStorageContainer"];

    public async Task<string?> ResolveAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var directFileName = TryGetFirstString(data, DirectFileNameKeys);
        if (!string.IsNullOrWhiteSpace(directFileName))
        {
            return ExtractFileName(directFileName);
        }

        var filePath = TryGetFirstString(data, FilePathKeys);
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var extractedFileName = ExtractFileName(filePath);
            if (!string.IsNullOrWhiteSpace(extractedFileName))
            {
                return extractedFileName;
            }
        }

        var blobServiceClient = blobServiceClientFactory.TryCreateClient();
        if (blobServiceClient is null)
        {
            return null;
        }

        var blobName = TryGetFirstString(data, ["blobName", "blobPath", "path"]);
        var containerName = TryGetFirstString(data, ContainerKeys) ?? configuration["CASE_SKILL_STORAGE_CONTAINER_NAME"];
        if (string.IsNullOrWhiteSpace(blobName) || string.IsNullOrWhiteSpace(containerName))
        {
            return null;
        }

        var blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName.TrimStart('/'));
        var exists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        return exists.Value ? ExtractFileName(blobClient.Name) : null;
    }

    public static string? ExtractFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            var fileNameFromUri = Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath));
            return string.IsNullOrWhiteSpace(fileNameFromUri) ? null : fileNameFromUri;
        }

        var normalizedPath = Uri.UnescapeDataString(value.Split(['?', '#'], 2)[0].TrimEnd('/').Replace('\\', '/'));
        var fileName = Path.GetFileName(normalizedPath);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }

    private static string? TryGetFirstString(JsonElement data, IEnumerable<string> keys)
    {
        if (data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var key in keys)
        {
            if (!data.TryGetProperty(key, out var value) || value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var stringValue = value.GetString();
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                return stringValue;
            }
        }

        return null;
    }
}
