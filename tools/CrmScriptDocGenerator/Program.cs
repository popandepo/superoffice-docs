using System.Text.Json;
using CrmScriptDocGenerator.Merge;
using CrmScriptDocGenerator.Parsers;
using CrmScriptDocGenerator.Writers;

var options = ParseArgs(args);

var markdownPath = options.GetValueOrDefault("markdown") ?? "docs/en/automation/crmscript";
var yamlPath = options.GetValueOrDefault("yaml") ?? "api/reference/crmscript";
var outputPath = options.GetValueOrDefault("output") ?? "output";

Directory.CreateDirectory(outputPath);
var wikiPath = Path.Combine(outputPath, "wiki");
var llmPath = Path.Combine(outputPath, "llm");
var searchIndexPath = Path.Combine(outputPath, "search-index.json");
var uidMapPath = Path.Combine(outputPath, "uid-map.json");

var markdownParser = new MarkdownParser();
var managedRefParser = new ManagedReferenceParser();
var merger = new DocumentationMerger();

var markdownDocs = markdownParser.ParseDirectory(markdownPath);
var apiItems = managedRefParser.ParseDirectory(yamlPath);
var merged = merger.Merge(markdownDocs, apiItems);

new WikiWriter().Write(wikiPath, merged);
var uidMap = new LlmWriter().Write(llmPath, merged);
new SearchIndexWriter().Write(searchIndexPath, apiItems);

File.WriteAllText(uidMapPath, JsonSerializer.Serialize(uidMap, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine($"Markdown docs: {markdownDocs.Count}");
Console.WriteLine($"API items: {apiItems.Count}");
Console.WriteLine($"Classes generated: {merged.Count}");
Console.WriteLine($"Output: {Path.GetFullPath(outputPath)}");

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var current = args[i];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        var value = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)
            ? args[++i]
            : "true";

        result[key] = value;
    }

    return result;
}

