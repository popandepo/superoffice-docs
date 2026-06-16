namespace CrmScriptDocGenerator.Models;

public sealed class MarkdownDoc
{
    public required string Uid { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string SourcePath { get; init; }
}

