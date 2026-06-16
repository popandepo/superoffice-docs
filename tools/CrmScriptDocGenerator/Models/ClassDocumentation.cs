namespace CrmScriptDocGenerator.Models;

public sealed class ClassDocumentation
{
    public required string ClassName { get; init; }
    public ApiItem? ClassItem { get; init; }
    public MarkdownDoc? Markdown { get; init; }
    public List<ApiItem> Members { get; init; } = [];
}

