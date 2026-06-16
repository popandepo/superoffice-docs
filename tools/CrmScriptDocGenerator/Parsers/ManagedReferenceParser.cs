using CrmScriptDocGenerator.Models;
using CrmScriptDocGenerator.Utils;
using YamlDotNet.Serialization;

namespace CrmScriptDocGenerator.Parsers;

public sealed class ManagedReferenceParser
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

    public Dictionary<string, ApiItem> ParseDirectory(string yamlRoot)
    {
        var items = new Dictionary<string, ApiItem>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(yamlRoot))
        {
            return items;
        }

        var yamlFiles = Directory.EnumerateFiles(yamlRoot, "*.y*ml", SearchOption.AllDirectories);
        foreach (var file in yamlFiles)
        {
            foreach (var item in ParseFile(file))
            {
                items[item.Uid] = item;
            }
        }

        return items;
    }

    private IEnumerable<ApiItem> ParseFile(string filePath)
    {
        var text = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var root = _deserializer.Deserialize<object>(text);
        if (root is not Dictionary<object, object> rootMap)
        {
            yield break;
        }

        if (TryGetDictionaryList(rootMap, "items", out var itemMaps))
        {
            foreach (var map in itemMaps)
            {
                var item = ToApiItem(map, filePath, rootMap);
                if (item is not null)
                {
                    yield return item;
                }
            }

            yield break;
        }

        var single = ToApiItem(rootMap, filePath, rootMap);
        if (single is not null)
        {
            yield return single;
        }
    }

    private static ApiItem? ToApiItem(Dictionary<object, object> map, string filePath, Dictionary<object, object> rootMap)
    {
        var uid = GetString(map, "uid");
        if (string.IsNullOrWhiteSpace(uid))
        {
            return null;
        }

        var syntax = TryGetMap(map, "syntax", out var syntaxMap) ? GetString(syntaxMap, "content") : null;
        var parameters = ParseParameters(map, syntaxMap);
        var returnType = ParseReturnType(map, syntaxMap);

        return new ApiItem
        {
            Uid = uid,
            Id = GetString(map, "id"),
            Name = GetString(map, "name"),
            FullName = GetString(map, "fullName"),
            Type = GetString(map, "type"),
            Summary = HtmlCleaner.CleanMarkdown(ExtractText(map, "summary")),
            Remarks = HtmlCleaner.CleanMarkdown(ExtractText(map, "remarks")),
            Syntax = syntax,
            Parameters = parameters,
            ReturnType = returnType,
            Examples = ParseExamples(map),
            Children = ParseStringList(map, "children"),
            References = ParseReferences(rootMap),
            Version = ParseSoValue(map, "version"),
            Intellisense = ParseSoValue(map, "intellisense"),
            SourcePath = filePath
        };
    }

    private static string? ParseSoValue(Dictionary<object, object> map, string key)
    {
        if (!TryGetMap(map, "so", out var soMap))
        {
            return null;
        }

        return GetString(soMap, key);
    }

    private static List<ApiParameter> ParseParameters(Dictionary<object, object> map, Dictionary<object, object>? syntaxMap)
    {
        var result = new List<ApiParameter>();
        var fromItem = ParseParameterList(map, "parameters");
        var fromSyntax = syntaxMap is null ? [] : ParseParameterList(syntaxMap, "parameters");

        foreach (var parameter in fromItem.Concat(fromSyntax))
        {
            if (result.All(p => !p.Id.Equals(parameter.Id, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(parameter);
            }
        }

        return result;
    }

    private static List<ApiParameter> ParseParameterList(Dictionary<object, object> map, string key)
    {
        var parameters = new List<ApiParameter>();
        if (!TryGetDictionaryList(map, key, out var list))
        {
            return parameters;
        }

        foreach (var node in list)
        {
            var id = GetString(node, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            parameters.Add(new ApiParameter
            {
                Id = id,
                Type = GetString(node, "type"),
                Description = HtmlCleaner.CleanMarkdown(ExtractText(node, "description"))
            });
        }

        return parameters;
    }

    private static string? ParseReturnType(Dictionary<object, object> map, Dictionary<object, object>? syntaxMap)
    {
        if (TryGetMap(map, "return", out var returnMap))
        {
            return GetString(returnMap, "type");
        }

        if (syntaxMap is not null && TryGetMap(syntaxMap, "return", out var syntaxReturnMap))
        {
            return GetString(syntaxReturnMap, "type");
        }

        return null;
    }

    private static List<string> ParseExamples(Dictionary<object, object> map)
    {
        var results = new List<string>();
        if (!TryGetObjectList(map, "example", out var list))
        {
            return results;
        }

        foreach (var item in list)
        {
            var text = item switch
            {
                string s => s,
                Dictionary<object, object> d => GetString(d, "content") ?? string.Empty,
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                results.Add(HtmlCleaner.CleanMarkdown(text));
            }
        }

        return results;
    }

    private static List<string> ParseReferences(Dictionary<object, object> rootMap)
    {
        var references = new List<string>();
        if (!TryGetDictionaryList(rootMap, "references", out var refs))
        {
            return references;
        }

        foreach (var node in refs)
        {
            var uid = GetString(node, "uid");
            if (!string.IsNullOrWhiteSpace(uid))
            {
                references.Add(uid);
            }
        }

        return references;
    }

    private static List<string> ParseStringList(Dictionary<object, object> map, string key)
    {
        var result = new List<string>();
        if (!TryGetObjectList(map, key, out var list))
        {
            return result;
        }

        foreach (var item in list)
        {
            if (item is string value && !string.IsNullOrWhiteSpace(value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static string? ExtractText(Dictionary<object, object> map, string key)
    {
        if (!TryGetValue(map, key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string text => text,
            Dictionary<object, object> contentMap => GetString(contentMap, "content"),
            List<object> list => string.Join("\n", list.Select(v => v?.ToString()).Where(v => !string.IsNullOrWhiteSpace(v))),
            _ => value.ToString()
        };
    }

    private static bool TryGetValue(Dictionary<object, object> map, string key, out object? value)
    {
        foreach (var pair in map)
        {
            if (pair.Key is string name && name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryGetMap(Dictionary<object, object> map, string key, out Dictionary<object, object> result)
    {
        result = [];
        if (!TryGetValue(map, key, out var value) || value is not Dictionary<object, object> found)
        {
            return false;
        }

        result = found;
        return true;
    }

    private static bool TryGetDictionaryList(Dictionary<object, object> map, string key, out List<Dictionary<object, object>> result)
    {
        result = [];
        if (!TryGetValue(map, key, out var value) || value is not List<object> list)
        {
            return false;
        }

        foreach (var node in list)
        {
            if (node is Dictionary<object, object> dictionary)
            {
                result.Add(dictionary);
            }
        }

        return result.Count > 0;
    }

    private static bool TryGetObjectList(Dictionary<object, object> map, string key, out List<object> result)
    {
        result = [];
        if (!TryGetValue(map, key, out var value) || value is not List<object> list)
        {
            return false;
        }

        result = list;
        return true;
    }

    private static string? GetString(Dictionary<object, object> map, string key)
    {
        if (!TryGetValue(map, key, out var value) || value is null)
        {
            return null;
        }

        return value.ToString();
    }

    private static string? GetString(object node, string key)
    {
        return node is Dictionary<object, object> map ? GetString(map, key) : null;
    }

    private static string? ExtractText(object node, string key)
    {
        return node is Dictionary<object, object> map ? ExtractText(map, key) : null;
    }
}
