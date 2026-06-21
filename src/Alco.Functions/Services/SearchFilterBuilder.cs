namespace Alco.Functions.Services;

public static class SearchFilterBuilder
{
    public static string BuildCaseIdFilter(string caseId) => $"caseId eq '{Escape(caseId)}'";

    private static string Escape(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
