using Alco.Functions.Models;
using Alco.Functions.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Alco.Functions.Tests;

public sealed class CaseSkillProcessorTests
{
    private static readonly IConfiguration EmptyConfiguration = new ConfigurationBuilder().Build();

    [Fact]
    public async Task ProcessAsync_ExtractsCaseIdFromMetadataStoragePath()
    {
        var processor = CreateProcessor();
        var request = new SearchSkillRequest
        {
            Values =
            [
                new SearchSkillRequestRecord
                {
                    RecordId = "1",
                    Data = ParseJson("""
                    {
                      "metadata_storage_path": "https://storage.example.com/docs/intake_packet_ABC123_4.pdf"
                    }
                    """)
                }
            ]
        };

        var response = await processor.ProcessAsync(request, CancellationToken.None);

        var record = Assert.Single(response.Values);
        Assert.NotNull(record.Data);
        Assert.Equal("ABC123", record.Data!.CaseId);
        Assert.Equal("intake_packet_ABC123_4.pdf", record.Data.FileName);
        Assert.Null(record.Errors);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsErrorWhenCaseIdCannotBeExtracted()
    {
        var processor = CreateProcessor();
        var request = new SearchSkillRequest
        {
            Values =
            [
                new SearchSkillRequestRecord
                {
                    RecordId = "2",
                    Data = ParseJson("""
                    {
                      "fileName": "notes_without_page_suffix.pdf"
                    }
                    """)
                }
            ]
        };

        var response = await processor.ProcessAsync(request, CancellationToken.None);

        var record = Assert.Single(response.Values);
        Assert.Null(record.Data);
        var error = Assert.Single(record.Errors!);
        Assert.Contains("Could not extract a case id", error.Message);
    }

    [Theory]
    [InlineData("folder/claim_summary_CASE-42_12.pdf", "CASE-42")]
    [InlineData("https://storage.example.com/folder/claim_summary_CASE777_001.pdf?sas=token", "CASE777")]
    public void TryExtract_ReturnsSecondToLastSegmentBeforePageNumber(string fileName, string expectedCaseId)
    {
        var parser = new CaseIdParser();

        var extracted = parser.TryExtract(fileName, out var caseId);

        Assert.True(extracted);
        Assert.Equal(expectedCaseId, caseId);
    }

    [Fact]
    public void BuildCaseIdFilter_EscapesSingleQuotes()
    {
        var filter = SearchFilterBuilder.BuildCaseIdFilter("CASE'42");

        Assert.Equal("caseId eq 'CASE''42'", filter);
    }

    private static CaseSkillProcessor CreateProcessor() =>
        new(new BlobFileNameResolver(new BlobServiceClientFactory(EmptyConfiguration), EmptyConfiguration), new CaseIdParser());

    private static JsonElement ParseJson(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
