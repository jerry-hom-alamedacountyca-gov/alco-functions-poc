using Alco.Functions.Models;

namespace Alco.Functions.Services;

public sealed class ContractSkillProcessor(BlobFileNameResolver fileNameResolver, ContractIdParser contractIdParser)
{
    public async Task<SearchSkillResponse> ProcessAsync(SearchSkillRequest request, CancellationToken cancellationToken)
    {
        var response = new SearchSkillResponse();

        foreach (var record in request.Values)
        {
            if (record.Data.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                response.Values.Add(CreateError(record.RecordId, "Each skill record must include a JSON object in the data field."));
                continue;
            }

            var fileName = await fileNameResolver.ResolveAsync(record.Data, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.Values.Add(CreateError(record.RecordId, "Could not determine a file name from the skill request payload."));
                continue;
            }

            if (!contractIdParser.TryExtract(fileName, out var contractId))
            {
                response.Values.Add(CreateError(record.RecordId, $"Could not extract a contract id from file name '{fileName}'."));
                continue;
            }

            response.Values.Add(new SearchSkillResponseRecord
            {
                RecordId = record.RecordId,
                Data = new ContractSkillResponseData
                {
                    ContractId = contractId!,
                    FileName = fileName
                }
            });
        }

        return response;
    }

    private static SearchSkillResponseRecord CreateError(string recordId, string message) =>
        new()
        {
            RecordId = recordId,
            Errors = [new SearchSkillMessage { Message = message }]
        };
}
