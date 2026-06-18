using Alco.Functions.Models;
using Alco.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Alco.Functions.Functions;

public sealed class GetCaseSkillFunction(CaseSkillProcessor processor, ILogger<GetCaseSkillFunction> logger)
{
    [Function("GetCaseSkill")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        SearchSkillRequest? skillRequest;

        try
        {
            skillRequest = await request.ReadFromJsonAsync<SearchSkillRequest>(cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Invalid skill payload received.");
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadRequest, new { error = "The request body must be valid JSON." }, cancellationToken).ConfigureAwait(false);
        }

        if (skillRequest is null)
        {
            return await CreateJsonResponseAsync(request, HttpStatusCode.BadRequest, new { error = "The request body is required." }, cancellationToken).ConfigureAwait(false);
        }

        var response = await processor.ProcessAsync(skillRequest, cancellationToken).ConfigureAwait(false);
        return await CreateJsonResponseAsync(request, HttpStatusCode.OK, response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HttpResponseData> CreateJsonResponseAsync<T>(HttpRequestData request, HttpStatusCode statusCode, T payload, CancellationToken cancellationToken)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(payload, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
