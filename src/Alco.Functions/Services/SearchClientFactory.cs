using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;

namespace Alco.Functions.Services;

public sealed class SearchClientFactory(IConfiguration configuration)
{
    public SearchClient CreateClient()
    {
        var endpoint = configuration["AI_SEARCH_ENDPOINT"];
        var indexName = configuration["AI_SEARCH_INDEX_NAME"];
        var apiKey = configuration["AI_SEARCH_API_KEY"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(indexName) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI Search is not configured. Set AI_SEARCH_ENDPOINT, AI_SEARCH_INDEX_NAME, and AI_SEARCH_API_KEY.");
        }

        return new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(apiKey));
    }
}
