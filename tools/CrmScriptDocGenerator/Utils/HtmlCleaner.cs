using System.Net;
using System.Text.RegularExpressions;

namespace CrmScriptDocGenerator.Utils;

public static class HtmlCleaner
{
    private static readonly Regex PreCodeRegex = new(
        "<pre><code(?: class=\\\"language-([^\\\"]+)\\\")?>(.*?)</code></pre>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex ParagraphRegex = new(
        "<p>(.*?)</p>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex BrRegex = new("<br\\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RemainingTagsRegex = new("</?[^>]+>", RegexOptions.Compiled);

    public static string CleanMarkdown(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var text = WebUtility.HtmlDecode(input);

        text = PreCodeRegex.Replace(text, match =>
        {
            var language = match.Groups[1].Success ? match.Groups[1].Value : "crmscript";
            var code = WebUtility.HtmlDecode(match.Groups[2].Value).Trim('\r', '\n');
            return $"```{language}\n{code}\n```";
        });

        text = ParagraphRegex.Replace(text, m => $"{m.Groups[1].Value}\n\n");
        text = BrRegex.Replace(text, "\n");
        text = RemainingTagsRegex.Replace(text, string.Empty);

        return text.Trim();
    }
}

