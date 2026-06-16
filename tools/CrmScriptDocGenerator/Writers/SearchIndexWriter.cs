using System.Text.Json;
using CrmScriptDocGenerator.Models;

namespace CrmScriptDocGenerator.Writers;

public sealed class SearchIndexWriter
{
    public void Write(string outputPath, IReadOnlyDictionary<string, ApiItem> apiByUid)
    {
        var entries = apiByUid.Values
            .OrderBy(v => v.FullName ?? v.Uid, StringComparer.OrdinalIgnoreCase)
            .Select(item => new
            {
                uid = item.Uid,
                name = item.Name,
                fullName = item.FullName,
                signature = item.Syntax,
                summary = item.Summary,
                remarks = item.Remarks,
                version = item.Version,
                intellisense = item.Intellisense,
                returnType = item.ReturnType,
                parameters = item.Parameters.Select(p => new { id = p.Id, type = p.Type, description = p.Description }).ToList()
            })
            .ToList();

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(outputPath, json);
    }
}

