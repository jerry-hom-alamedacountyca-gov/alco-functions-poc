using Alco.Functions.Models;
using Alco.Functions.Services;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Alco.Functions.Functions;

public sealed class QueryAiSearchSkillFunction(SearchClientFactory searchClientFactory, ILogger<QueryAiSearchSkillFunction> logger)
{
    [Function("QueryAiSearchSkill")]
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

        if (queryRequest is null || string.IsNullOrWhiteSpace(queryRequest.Query) || string.IsNullOrWhiteSpace(queryRequest.CaseId))
        {
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadRequest, new { error = "Both query and caseId are required." }, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var searchClient = searchClientFactory.CreateClient();
            var searchOptions = new SearchOptions
            {
                Filter = SearchFilterBuilder.BuildCaseIdFilter(queryRequest.CaseId),
                IncludeTotalCount = true,
                Size = NormalizeSize(queryRequest.Top)
            };

            if (queryRequest.Select is { Count: > 0 })
            {
                foreach (var field in queryRequest.Select.Where(static field => !string.IsNullOrWhiteSpace(field)))
                {
                    searchOptions.Select.Add(field);
                }
            }

            var results = await searchClient.SearchAsync<SearchDocument>(queryRequest.Query, searchOptions, cancellationToken).ConfigureAwait(false);
            var documents = new List<SearchDocument>();
            await foreach (var result in results.Value.GetResultsAsync())
            {
                documents.Add(result.Document);
            }

            return await CreateJsonResponseAsync(
                request,
                HttpStatusCode.OK,
                new QueryAiSearchResponse
                {
                    CaseId = queryRequest.CaseId,
                    Filter = searchOptions.Filter,
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
