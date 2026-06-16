# CRMScript Documentation Merger / Generator

## Goal

Create a tool that merges the two documentation sources from the SuperOffice documentation repository into a much more useful local documentation system.

The generated documentation should serve two purposes:

1. Human-readable wiki for browsing and searching.
2. LLM-optimized documentation for RAG/embeddings.

---

## Background

The SuperOffice CRMScript documentation is split into two different formats.

### Markdown

Contains:

- explanations
- tutorials
- conceptual documentation
- examples
- notes
- navigation
- xrefs

Example:

````md
---
uid: crmscript_eventdata
title: EventData
---

# EventData

EventData gives you access to contextual information...

## Example

```crmscript
EventData ed = getEventData();
...
```

See:

[xref:CRMScript.Native.EventData.getInputValue(String)](xref:CRMScript.Native.EventData.getInputValue%28String%29)
````

### YAML (ManagedReference)

Contains structured API metadata.

Example:

```yaml
uid: CRMScript.Native.EventData.getInputValue(String)

name: getInputValue(String)

fullName: CRMScript.Native.EventData.getInputValue(String)

summary:
  Returns the value of a specified input field.

syntax:
  content: String getInputValue(String field)

parameters:
  - id: field
    type: CRMScript.Global.String

return:
  type: CRMScript.Global.String
```

The YAML contains nearly everything required to build an API reference:

- uid
- name
- fullName
- type
- summary
- remarks
- examples
- syntax
- parameters
- return type
- version
- intellisense name
- child members
- references

---

## Desired Inputs

```text
Markdown Folder
    docs/en/automation/crmscript/

YAML Folder
    api/reference/crmscript/
```

Both folders should be recursively scanned.

---

## Desired Outputs

```text
output/

    wiki/
    llm/
    search-index.json
    uid-map.json
```

---

## Output 1 - Human Wiki

Purpose:

Human browsing.

One page per class.

Example:

```text
wiki/

    EventData.md
    Ticket.md
    SearchEngine.md
```

Each page should contain:

- class summary
- description from markdown
- methods table
- properties
- examples
- remarks
- notes
- navigation
- links to related classes

Method sections should include:

```text
## getInputValue()

Summary

Signature

Parameters

Returns

Remarks

Examples
```

Markdown should be cleaned:

- remove frontmatter
- convert HTML entities
- convert `<pre><code>`
- preserve markdown formatting
- preserve code blocks

---

## Output 2 - LLM Documentation

Purpose:

Embeddings / RAG.

Instead of one large page, create one file per API member.

Example:

```text
llm/

    EventData/

        class.md

        getInputValue.md

        getInputValues.md

        setOutputValue.md
```

Each file should be dense and consistent.

Example:

```md
ENTITY_TYPE: METHOD

CLASS: EventData

UID:
CRMScript.Native.EventData.getInputValue(String)

NAME:
getInputValue

SIGNATURE:

String getInputValue(String field)

SUMMARY:

Returns the value of a specified input field.

PARAMETERS

field (String)

RETURNS

String

REMARKS

Use Trace...

EXAMPLES

...
```

No navigation.

No duplicated information.

Optimized for semantic search.

---

## UID Resolution

Markdown uses xrefs.

Example:

```text
<xref:CRMScript.Native.EventData.getInputValue(String)>
```

YAML uses:

```text
uid:
CRMScript.Native.EventData.getInputValue(String)
```

These should be merged.

Generate:

```text
uid-map.json
```

Example:

```json
{
  "CRMScript.Native.EventData.getInputValue(String)": {
    "class": "EventData",
    "member": "getInputValue",
    "path": "llm/EventData/getInputValue.md"
  }
}
```

---

## Search Index

Generate:

```text
search-index.json
```

Each entry should contain:

```json
{
  "uid": "...",
  "name": "...",
  "fullName": "...",
  "signature": "...",
  "summary": "...",
  "remarks": "...",
  "version": "...",
  "intellisense": "...",
  "returnType": "...",
  "parameters": []
}
```

This enables exact lookup without embeddings.

---

## Parsing

Markdown:

Extract:

- uid
- title
- body

Remove frontmatter.

YAML:

Deserialize using YamlDotNet.

Relevant fields:

```text
uid
id
name
fullName
type
summary
remarks
syntax
parameters
return
example
children
references
so.version
so.intellisense
```

Ignore unknown properties.

---

## Merge Strategy

1. Load every markdown file.

Store by UID.

```text
Dictionary<string, MarkdownDoc>
```

2. Load every YAML file.

Store every API item by UID.

```text
Dictionary<string, ApiItem>
```

3. For every API item:

- locate matching markdown
- merge
- generate wiki page
- generate LLM page

---

## Cleaning

Convert:

```text
<pre><code>
```

to:

````md
```crmscript
```
````

Convert:

```text
&quot;
&lt;
&gt;
&amp;
```

to normal characters.

Convert:

```text
<p></p>
```

into blank lines.

Remove unnecessary HTML.

---

## Nice Features

Generate:

- Class index
- Function index
- Method count
- Version information
- Cross links
- Related classes
- Breadcrumbs

---

## Future Features

### Static Documentation Website

Generate a complete static documentation site.

Possible generators:

- MkDocs
- Docusaurus
- Hugo

### Full-text Search

Generate Lunr.js or Pagefind index.

Allows instant browser search.

### LLM Enhancements

Generate one embedding chunk per:

- class
- method
- enum
- property

Avoid chunks larger than ~2 KB.

### Relationship Graph

Generate graph showing:

```text
Class

↓

Methods

↓

Referenced Classes

↓

Examples

↓

Related Tutorials
```

Useful for AI navigation.

---

## Suggested Project Structure

```text
CrmScriptDocGenerator/

    Models/

        MarkdownDoc.cs
        ApiItem.cs

    Parsers/

        MarkdownParser.cs
        ManagedReferenceParser.cs

    Merge/

        DocumentationMerger.cs

    Writers/

        WikiWriter.cs
        LlmWriter.cs
        SearchIndexWriter.cs

    Utils/

        HtmlCleaner.cs
        XrefResolver.cs

    Program.cs
```

---

## NuGet

Required:

```text
YamlDotNet
```

License:

MIT

---

## Success Criteria

The finished tool should provide a significantly better developer experience than the official SuperOffice documentation by:

- exact function search
- offline documentation
- better navigation
- merged conceptual + API docs
- AI-friendly documentation
- human-friendly documentation
- searchable JSON indexes
- reusable RAG corpus

The resulting documentation should effectively become a replacement for the official CRMScript documentation site while remaining fully generated from the original source files.

## Quick start

From repo root:

```powershell
dotnet restore .\tools\CrmScriptDocGenerator\CrmScriptDocGenerator.csproj
dotnet run --project .\tools\CrmScriptDocGenerator\CrmScriptDocGenerator.csproj -- --markdown .\tools\CrmScriptDocGenerator\SampleData\markdown --yaml .\tools\CrmScriptDocGenerator\SampleData\yaml --output .\tools\CrmScriptDocGenerator\SampleData\output
```

Run against real docs (paths from this repository):

```powershell
dotnet run --project .\tools\CrmScriptDocGenerator\CrmScriptDocGenerator.csproj -- --markdown .\docs\en\automation\crmscript --yaml .\api\reference\crmscript --output .\tools\CrmScriptDocGenerator\output
```
