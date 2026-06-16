using System.Text;
using CrmScriptDocGenerator.Models;

namespace CrmScriptDocGenerator.Writers;

public sealed class LlmWriter
{
    public Dictionary<string, object> Write(string llmOutputPath, IReadOnlyList<ClassDocumentation> classes)
    {
        Directory.CreateDirectory(llmOutputPath);
        var uidMap = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var classDoc in classes)
        {
            var classDir = Path.Combine(llmOutputPath, classDoc.ClassName);
            Directory.CreateDirectory(classDir);

            File.WriteAllText(Path.Combine(classDir, "class.md"), BuildClassChunk(classDoc));

            foreach (var member in classDoc.Members)
            {
                var fileName = $"{member.MemberName}.md";
                var memberPath = Path.Combine(classDir, fileName);
                File.WriteAllText(memberPath, BuildMemberChunk(classDoc.ClassName, member));

                uidMap[member.Uid] = new
                {
                    @class = classDoc.ClassName,
                    member = member.MemberName,
                    path = Path.Combine("llm", classDoc.ClassName, fileName).Replace('\\', '/')
                };
            }

            if (classDoc.ClassItem is not null)
            {
                uidMap[classDoc.ClassItem.Uid] = new
                {
                    @class = classDoc.ClassName,
                    member = "class",
                    path = Path.Combine("llm", classDoc.ClassName, "class.md").Replace('\\', '/')
                };
            }
        }

        return uidMap;
    }

    private static string BuildClassChunk(ClassDocumentation classDoc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ENTITY_TYPE: CLASS");
        sb.AppendLine();
        sb.AppendLine($"CLASS: {classDoc.ClassName}");
        sb.AppendLine();
        sb.AppendLine("UID:");
        sb.AppendLine(classDoc.ClassItem?.Uid ?? classDoc.ClassName);
        sb.AppendLine();
        sb.AppendLine("SUMMARY:");
        sb.AppendLine();
        sb.AppendLine(classDoc.ClassItem?.Summary ?? string.Empty);
        sb.AppendLine();
        sb.AppendLine("DESCRIPTION:");
        sb.AppendLine();
        sb.AppendLine(classDoc.Markdown?.Body ?? string.Empty);
        return sb.ToString().Trim() + Environment.NewLine;
    }

    private static string BuildMemberChunk(string className, ApiItem member)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ENTITY_TYPE: METHOD");
        sb.AppendLine();
        sb.AppendLine($"CLASS: {className}");
        sb.AppendLine();
        sb.AppendLine("UID:");
        sb.AppendLine(member.Uid);
        sb.AppendLine();
        sb.AppendLine("NAME:");
        sb.AppendLine(member.MemberName);
        sb.AppendLine();
        sb.AppendLine("SIGNATURE:");
        sb.AppendLine();
        sb.AppendLine(member.Syntax ?? string.Empty);
        sb.AppendLine();
        sb.AppendLine("SUMMARY:");
        sb.AppendLine();
        sb.AppendLine(member.Summary ?? string.Empty);
        sb.AppendLine();
        sb.AppendLine("PARAMETERS:");
        sb.AppendLine();
        foreach (var p in member.Parameters)
        {
            sb.AppendLine($"- {p.Id} ({p.Type ?? "unknown"})");
        }

        sb.AppendLine();
        sb.AppendLine("RETURNS:");
        sb.AppendLine();
        sb.AppendLine(member.ReturnType ?? string.Empty);
        sb.AppendLine();
        sb.AppendLine("REMARKS:");
        sb.AppendLine();
        sb.AppendLine(member.Remarks ?? string.Empty);
        sb.AppendLine();
        sb.AppendLine("EXAMPLES:");
        sb.AppendLine();

        foreach (var ex in member.Examples)
        {
            sb.AppendLine(ex);
            sb.AppendLine();
        }

        return sb.ToString().Trim() + Environment.NewLine;
    }
}

