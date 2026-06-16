using System.Text;
using System.Linq;
using CrmScriptDocGenerator.Models;

namespace CrmScriptDocGenerator.Writers;

public sealed class WikiWriter
{
    public void Write(string wikiOutputPath, IReadOnlyList<ClassDocumentation> classes)
    {
        Directory.CreateDirectory(wikiOutputPath);

        foreach (var classDoc in classes)
        {
            var filePath = Path.Combine(wikiOutputPath, $"{classDoc.ClassName}.md");
            File.WriteAllText(filePath, BuildClassPage(classDoc));
        }

        File.WriteAllText(Path.Combine(wikiOutputPath, "index.md"), BuildIndex(classes));
    }

    private static string BuildClassPage(ClassDocumentation classDoc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {classDoc.ClassName}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(classDoc.ClassItem?.Summary))
        {
            sb.AppendLine(classDoc.ClassItem.Summary);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(classDoc.Markdown?.Body))
        {
            sb.AppendLine(classDoc.Markdown.Body);
            sb.AppendLine();
        }

        if (classDoc.Members.Count > 0)
        {
            sb.AppendLine("## Methods");
            sb.AppendLine();
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("| --- | --- |");
            foreach (var member in classDoc.Members)
            {
                var summary = string.IsNullOrWhiteSpace(member.Summary) ? "" : member.Summary.Replace("\n", " ");
                var heading = GetMemberHeading(member);
                var anchor = GetMemberAnchor(member);
                sb.AppendLine($"| [`{heading}`](#{anchor}) | {summary} |");
            }

            sb.AppendLine();
        }

        foreach (var member in classDoc.Members)
        {
            var heading = GetMemberHeading(member);
            var anchor = GetMemberAnchor(member);
            sb.AppendLine($"<a id=\"{anchor}\"></a>");
            sb.AppendLine();
            sb.AppendLine($"## {heading}");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(member.Summary))
            {
                sb.AppendLine("### Summary");
                sb.AppendLine();
                sb.AppendLine(member.Summary);
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(member.Syntax))
            {
                sb.AppendLine("### Signature");
                sb.AppendLine();
                sb.AppendLine("```crmscript");
                sb.AppendLine(member.Syntax);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (member.Parameters.Count > 0)
            {
                sb.AppendLine("### Parameters");
                sb.AppendLine();
                foreach (var parameter in member.Parameters)
                {
                    sb.AppendLine($"- `{parameter.Id}` ({parameter.Type ?? "unknown"}) {parameter.Description}");
                }

                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(member.ReturnType))
            {
                sb.AppendLine("### Returns");
                sb.AppendLine();
                sb.AppendLine(member.ReturnType);
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(member.Remarks))
            {
                sb.AppendLine("### Remarks");
                sb.AppendLine();
                sb.AppendLine(member.Remarks);
                sb.AppendLine();
            }

            if (member.Examples.Count > 0)
            {
                sb.AppendLine("### Examples");
                sb.AppendLine();
                foreach (var example in member.Examples)
                {
                    sb.AppendLine(example);
                    sb.AppendLine();
                }
            }
        }

        return sb.ToString().Trim() + Environment.NewLine;
    }

    private static string BuildIndex(IReadOnlyList<ClassDocumentation> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Class Index");
        sb.AppendLine();
        sb.AppendLine($"Total classes: **{classes.Count}**");
        sb.AppendLine();

        foreach (var classDoc in classes)
        {
            sb.AppendLine($"- [{classDoc.ClassName}]({classDoc.ClassName}.md) ({classDoc.Members.Count} members)");
        }

        return sb.ToString();
    }

    private static string GetMemberHeading(ApiItem member)
    {
        return string.IsNullOrWhiteSpace(member.Name) ? $"{member.MemberName}()" : member.Name;
    }

    private static string GetMemberAnchor(ApiItem member)
    {
        var source = string.IsNullOrWhiteSpace(member.Uid) ? member.MemberName : member.Uid;
        var chars = source
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        return $"m-{new string(chars).Trim('-')}";
    }
}

