using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Alco.Functions.Services;

public sealed class BlobServiceClientFactory
{
    private readonly BlobServiceClient? _blobServiceClient;

    public BlobServiceClientFactory(IConfiguration configuration)
    {
        var connectionString = configuration["CASE_SKILL_STORAGE_CONNECTION_STRING"];
        _blobServiceClient = string.IsNullOrWhiteSpace(connectionString)
            ? null
            : new BlobServiceClient(connectionString);
    }

    public BlobServiceClient? TryCreateClient() => _blobServiceClient;
}
