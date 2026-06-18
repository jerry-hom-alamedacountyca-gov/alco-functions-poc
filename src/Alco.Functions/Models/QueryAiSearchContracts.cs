using Azure.Search.Documents.Models;
using System.Text.Json.Serialization;

namespace Alco.Functions.Models;

public sealed class QueryAiSearchRequest
{
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    [JsonPropertyName("caseId")]
    public string CaseId { get; init; } = string.Empty;

    [JsonPropertyName("top")]
    public int? Top { get; init; }

    [JsonPropertyName("select")]
    public List<string>? Select { get; init; }
}

public sealed class QueryAiSearchResponse
{
    [JsonPropertyName("caseId")]
    public string CaseId { get; init; } = string.Empty;

    [JsonPropertyName("filter")]
    public string Filter { get; init; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("totalCount")]
    public long? TotalCount { get; init; }

    [JsonPropertyName("results")]
    public List<SearchDocument> Results { get; init; } = [];
}
