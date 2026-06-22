using Alco.Functions.Models;
using Alco.Functions.Services;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;

namespace Alco.Functions.Functions;

public sealed class QueryAiSearchSkillFunction(SearchClientFactory searchClientFactory, ILogger<QueryAiSearchSkillFunction> logger)
{
    [Function("QueryAiSearchSkill")]
    [OpenApiOperation(
        operationId: "QueryContractDocuments",
        tags: ["Search"],
        Summary = "Search documents for a contract",
        Description = "Performs a semantic search over indexed document chunks scoped to a specific contract ID. Returns the most relevant text chunks along with their source file metadata.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(QueryAiSearchRequest), Required = true, Description = "The search query and contract ID to scope results to.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(QueryAiSearchResponse), Description = "Matching document chunks for the contract.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(object), Description = "query or contractId was missing or invalid.")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        QueryAiSearchRequest? queryRequest;

        try
        {
            queryRequest = await request.ReadFromJsonAsync<QueryAiSearchRequest>(cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Invalid AI Search query payload received.");
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadRequest, new { error = "The request body must be valid JSON." }, cancellationToken).ConfigureAwait(false);
        }

        if (queryRequest is null || string.IsNullOrWhiteSpace(queryRequest.Query) || string.IsNullOrWhiteSpace(queryRequest.ContractId))
        {
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadRequest, new { error = "Both query and contractId are required." }, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var searchClient = searchClientFactory.CreateClient();
            var top = NormalizeSize(queryRequest.Top);
            var searchOptions = new SearchOptions
            {
                Filter = SearchFilterBuilder.BuildContractIdFilter(queryRequest.ContractId),
                IncludeTotalCount = true,
                Size = top,
                VectorSearch = new VectorSearchOptions
                {
                    Queries =
                    {
                        new VectorizableTextQuery(queryRequest.Query)
                        {
                            Fields = { "text_vector" },
                            KNearestNeighborsCount = top
                        }
                    }
                }
            };
            searchOptions.Select.Add("chunk");
            searchOptions.Select.Add("title");
            searchOptions.Select.Add("contractId");
            searchOptions.Select.Add("fileName");

            var results = await searchClient.SearchAsync<SearchDocument>(queryRequest.Query, searchOptions, cancellationToken).ConfigureAwait(false);
            var documents = new List<ContractSearchResult>();
            await foreach (var result in results.Value.GetResultsAsync())
            {
                documents.Add(new ContractSearchResult
                {
                    Chunk = result.Document.TryGetValue("chunk", out var chunk) ? chunk?.ToString() ?? string.Empty : string.Empty,
                    Title = result.Document.TryGetValue("title", out var title) ? title?.ToString() ?? string.Empty : string.Empty,
                    ContractId = result.Document.TryGetValue("contractId", out var contractId) ? contractId?.ToString() ?? string.Empty : string.Empty,
                    FileName = result.Document.TryGetValue("fileName", out var fileName) ? fileName?.ToString() ?? string.Empty : string.Empty
                });
            }

            return await CreateJsonResponseAsync(
                request,
                HttpStatusCode.OK,
                new QueryAiSearchResponse
                {
                    ContractId = queryRequest.ContractId,
                    Count = documents.Count,
                    TotalCount = results.Value.TotalCount,
                    Results = documents
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "AI Search configuration is incomplete.");
            return await CreateJsonResponseAsync(request, HttpStatusCode.InternalServerError, new { error = exception.Message }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "AI Search query failed.");
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadGateway, new { error = "The AI Search query failed." }, cancellationToken).ConfigureAwait(false);
        }
    }

    private static int NormalizeSize(int? top)
    {
        if (!top.HasValue || top.Value <= 0)
        {
            return 5;
        }

        return Math.Min(top.Value, 50);
    }

    private static async Task<HttpResponseData> CreateJsonResponseAsync<T>(HttpRequestData request, HttpStatusCode statusCode, T payload, CancellationToken cancellationToken)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(payload, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
