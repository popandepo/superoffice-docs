using CrmScriptDocGenerator.Models;

namespace CrmScriptDocGenerator.Merge;

public sealed class DocumentationMerger
{
    public IReadOnlyList<ClassDocumentation> Merge(
        IReadOnlyDictionary<string, MarkdownDoc> markdownByUid,
        IReadOnlyDictionary<string, ApiItem> apiByUid)
    {
        var classGroups = apiByUid.Values
            .GroupBy(i => i.ClassName, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        var markdownByTitle = markdownByUid.Values
            .GroupBy(d => d.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var markdownByKey = markdownByUid.Values
            .ToDictionary(d => d.Uid, d => d, StringComparer.OrdinalIgnoreCase);

        var docs = new List<ClassDocumentation>();

        foreach (var classGroup in classGroups)
        {
            var classItem = classGroup.FirstOrDefault(i =>
                string.Equals(i.Type, "Class", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.MemberName, classGroup.Key, StringComparison.OrdinalIgnoreCase));

            var markdown = ResolveMarkdown(classGroup.Key, classItem, markdownByTitle, markdownByKey);

            docs.Add(new ClassDocumentation
            {
                ClassName = classGroup.Key,
                ClassItem = classItem,
                Markdown = markdown,
                Members = classGroup
                    .Where(i => !string.Equals(i.Type, "Class", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.MemberName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            });
        }

        return docs;
    }

    private static MarkdownDoc? ResolveMarkdown(
        string className,
        ApiItem? classItem,
        IReadOnlyDictionary<string, MarkdownDoc> markdownByTitle,
        IReadOnlyDictionary<string, MarkdownDoc> markdownByUid)
    {
        if (classItem is not null && markdownByUid.TryGetValue(classItem.Uid, out var directMatch))
        {
            return directMatch;
        }

        if (markdownByTitle.TryGetValue(className, out var byTitle))
        {
            return byTitle;
        }

        var fallbackKey = $"crmscript_{className}";
        return markdownByUid.TryGetValue(fallbackKey, out var fallback) ? fallback : null;
    }
}

