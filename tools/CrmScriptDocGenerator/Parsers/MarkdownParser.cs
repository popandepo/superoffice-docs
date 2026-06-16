using System.Text.RegularExpressions;
using CrmScriptDocGenerator.Models;
using CrmScriptDocGenerator.Utils;

namespace CrmScriptDocGenerator.Parsers;

public sealed class MarkdownParser
{
    private static readonly Regex FrontmatterRegex = new(
        "^---\\s*\\r?\\n(.*?)\\r?\\n---\\s*\\r?\\n",
        RegexOptions.Singleline | RegexOptions.Compiled);

    public Dictionary<string, MarkdownDoc> ParseDirectory(string markdownRoot)
    {
        var docs = new Dictionary<string, MarkdownDoc>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(markdownRoot))
        {
            return docs;
        }

        foreach (var file in Directory.EnumerateFiles(markdownRoot, "*.md", SearchOption.AllDirectories))
        {
            var parsed = ParseFile(file);
            if (parsed is null)
            {
                continue;
            }

            docs[parsed.Uid] = parsed;
        }

        return docs;
    }

    private static MarkdownDoc? ParseFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var uid = string.Empty;
        var title = Path.GetFileNameWithoutExtension(filePath);
        var body = content;

        var match = FrontmatterRegex.Match(content);
        if (match.Success)
        {
            var frontmatter = match.Groups[1].Value;
            body = content[match.Length..];

            foreach (var line in frontmatter.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("uid:", StringComparison.OrdinalIgnoreCase))
                {
                    uid = trimmed[4..].Trim();
                }
                else if (trimmed.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                {
                    title = trimmed[6..].Trim();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(uid))
        {
            return null;
        }

        return new MarkdownDoc
        {
            Uid = uid,
            Title = title,
            Body = HtmlCleaner.CleanMarkdown(body),
            SourcePath = filePath
        };
    }
}

