namespace CrmScriptDocGenerator.Models;

public sealed class ApiItem
{
    public required string Uid { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? FullName { get; init; }
    public string? Type { get; init; }
    public string? Summary { get; init; }
    public string? Remarks { get; init; }
    public string? Syntax { get; init; }
    public List<ApiParameter> Parameters { get; init; } = [];
    public string? ReturnType { get; init; }
    public List<string> Examples { get; init; } = [];
    public List<string> Children { get; init; } = [];
    public List<string> References { get; init; } = [];
    public string? Version { get; init; }
    public string? Intellisense { get; init; }
    public required string SourcePath { get; init; }

    public string ClassName => NameHelpers.GetClassName(this);
    public string MemberName => NameHelpers.GetMemberName(this);
}

public sealed class ApiParameter
{
    public required string Id { get; init; }
    public string? Type { get; init; }
    public string? Description { get; init; }
}

internal static class NameHelpers
{
    public static string GetClassName(ApiItem item)
    {
        var source = item.FullName ?? item.Uid;
        var trimmed = source.Split('(')[0];
        var lastDot = trimmed.LastIndexOf('.');
        if (lastDot < 0)
        {
            return trimmed;
        }

        var tail = trimmed[(lastDot + 1)..];
        var isClassLike = string.Equals(item.Type, "Class", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(item.Type, "Struct", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(item.Type, "Interface", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(item.Type, "Enum", StringComparison.OrdinalIgnoreCase);
        if (isClassLike)
        {
            return tail;
        }

        var parent = trimmed[..lastDot];

        var classDot = parent.LastIndexOf('.');
        return classDot >= 0 ? parent[(classDot + 1)..] : parent;
    }

    public static string GetMemberName(ApiItem item)
    {
        var source = item.Name ?? item.Id ?? item.Uid;
        var noParams = source.Split('(')[0];
        return noParams.Contains('.') ? noParams[(noParams.LastIndexOf('.') + 1)..] : noParams;
    }
}

