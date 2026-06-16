using System.Text.RegularExpressions;

namespace CrmScriptDocGenerator.Utils;

public static class XrefResolver
{
    private static readonly Regex XrefPattern = new(
        "xref:([^)>\\s]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyCollection<string> ExtractUids(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in XrefPattern.Matches(text))
        {
            var uid = Uri.UnescapeDataString(match.Groups[1].Value).Trim();
            if (!string.IsNullOrWhiteSpace(uid))
            {
                result.Add(uid);
            }
        }

        return result.ToList();
    }
}

