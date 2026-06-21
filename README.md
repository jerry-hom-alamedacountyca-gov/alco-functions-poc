# alco-functions-poc

.NET 10 Azure Functions isolated worker app with two HTTP-triggered functions:

- `GetCaseSkill`: accepts Azure AI Search custom skill batch payloads, resolves a file name from the request data, and returns an extracted `caseId` from file names shaped like `[filename]_[etc]_[case id]_[page #].pdf`.
- `QueryAiSearchSkill`: accepts a JSON payload from AI Foundry and runs an Azure AI Search query filtered to a specific `caseId`.

## Configuration

Set these environment variables before running the functions:

- `AI_SEARCH_ENDPOINT`
- `AI_SEARCH_INDEX_NAME`
- `AI_SEARCH_API_KEY`
- `CASE_SKILL_STORAGE_CONNECTION_STRING` *(optional, only used when the request payload cannot provide a file name directly and blob lookup is needed)*
- `CASE_SKILL_STORAGE_CONTAINER_NAME` *(optional default container for blob lookup)*

## Request shapes

### GetCaseSkill

`POST /api/GetCaseSkill`

```json
{
  "values": [
    {
      "recordId": "1",
      "data": {
        "metadata_storage_path": "https://storage.example.com/docs/intake_packet_ABC123_4.pdf"
      }
    }
  ]
}
```

Successful response values contain:

```json
{
  "recordId": "1",
  "data": {
    "caseId": "ABC123",
    "fileName": "intake_packet_ABC123_4.pdf"
  }
}
```

### QueryAiSearchSkill

`POST /api/QueryAiSearchSkill`

```json
{
  "query": "hearing transcript",
  "caseId": "ABC123",
  "top": 5,
  "select": ["id", "title", "content", "caseId"]
}
```
