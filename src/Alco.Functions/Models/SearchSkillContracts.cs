using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alco.Functions.Models;

public sealed class SearchSkillRequest
{
    [JsonPropertyName("values")]
    public List<SearchSkillRequestRecord> Values { get; init; } = [];
}

public sealed class SearchSkillRequestRecord
{
    [JsonPropertyName("recordId")]
    public string RecordId { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}

public sealed class SearchSkillResponse
{
    [JsonPropertyName("values")]
    public List<SearchSkillResponseRecord> Values { get; init; } = [];
}

public sealed class SearchSkillResponseRecord
{
    [JsonPropertyName("recordId")]
    public string RecordId { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CaseSkillResponseData? Data { get; init; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SearchSkillMessage>? Errors { get; init; }

    [JsonPropertyName("warnings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SearchSkillMessage>? Warnings { get; init; }
}

public sealed class SearchSkillMessage
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}

public sealed class CaseSkillResponseData
{
    [JsonPropertyName("caseId")]
    public string CaseId { get; init; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;
}
