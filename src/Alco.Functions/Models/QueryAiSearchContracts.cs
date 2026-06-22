using System.Text.Json.Serialization;

namespace Alco.Functions.Models;

public sealed class QueryAiSearchRequest
{
    /// <summary>The natural language search query to find relevant document chunks.</summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>The contract ID to scope the search to a specific contract's documents.</summary>
    [JsonPropertyName("contractId")]
    public string ContractId { get; init; } = string.Empty;

    /// <summary>Maximum number of results to return. Defaults to 5, maximum 50.</summary>
    [JsonPropertyName("top")]
    public int? Top { get; init; }
}

public sealed class QueryAiSearchResponse
{
    /// <summary>The contract ID used to filter the search.</summary>
    [JsonPropertyName("contractId")]
    public string ContractId { get; init; } = string.Empty;

    /// <summary>Number of results returned.</summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>Total number of matching documents in the index.</summary>
    [JsonPropertyName("totalCount")]
    public long? TotalCount { get; init; }

    /// <summary>Matching document chunks with their contract metadata.</summary>
    [JsonPropertyName("results")]
    public List<ContractSearchResult> Results { get; init; } = [];
}

public sealed class ContractSearchResult
{
    /// <summary>The text content of the document chunk.</summary>
    [JsonPropertyName("chunk")]
    public string Chunk { get; init; } = string.Empty;

    /// <summary>The document file name.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>The contract ID this document belongs to.</summary>
    [JsonPropertyName("contractId")]
    public string ContractId { get; init; } = string.Empty;

    /// <summary>The blob storage file name of the source document.</summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;
}
