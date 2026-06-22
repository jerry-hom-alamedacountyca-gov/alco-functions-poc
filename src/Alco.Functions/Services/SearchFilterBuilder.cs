namespace Alco.Functions.Services;

public static class SearchFilterBuilder
{
    public static string BuildContractIdFilter(string contractId) => $"contractId eq '{Escape(contractId)}'";

    private static string Escape(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
