using Alco.Functions.Models;

namespace Alco.Functions.Services;

public sealed class CaseSkillProcessor(BlobFileNameResolver fileNameResolver, CaseIdParser caseIdParser)
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

            if (!caseIdParser.TryExtract(fileName, out var caseId))
            {
                response.Values.Add(CreateError(record.RecordId, $"Could not extract a case id from file name '{fileName}'."));
                continue;
            }

            response.Values.Add(new SearchSkillResponseRecord
            {
                RecordId = record.RecordId,
                Data = new CaseSkillResponseData
                {
                    CaseId = caseId!,
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
