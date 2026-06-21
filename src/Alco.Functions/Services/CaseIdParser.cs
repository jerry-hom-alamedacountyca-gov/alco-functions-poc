namespace Alco.Functions.Services;

public sealed class CaseIdParser
{
    public bool TryExtract(string fileName, out string? caseId)
    {
        caseId = null;

        var normalizedFileName = BlobFileNameResolver.ExtractFileName(fileName) ?? fileName;
        var baseName = Path.GetFileNameWithoutExtension(normalizedFileName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            return false;
        }

        var segments = baseName.Split('_', StringSplitOptions.TrimEntries);
        if (segments.Length < 3)
        {
            return false;
        }

        var pageSegment = segments[^1];
        if (!int.TryParse(pageSegment, out _))
        {
            return false;
        }

        caseId = segments[^2];
        return !string.IsNullOrWhiteSpace(caseId);
    }
}
