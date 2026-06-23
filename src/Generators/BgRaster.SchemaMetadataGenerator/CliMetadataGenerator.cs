// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GameshowPro.BgRaster.SchemaMetadataGenerator;

[Generator]
public sealed class CliMetadataGenerator : IIncrementalGenerator
{
    static readonly DiagnosticDescriptor MissingSchemaDiagnostic = new(
        id: "GSPGEN001",
        title: "Schema metadata file not found",
        messageFormat: "Could not locate bgraster-config.schema.json in AdditionalFiles. Generated CLI metadata will be empty.",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor InvalidSchemaDiagnostic = new(
        id: "GSPGEN002",
        title: "Schema metadata parse failure",
        messageFormat: "Could not parse x-bgraster metadata from bgraster-config.schema.json: {0}",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> schemaFiles = context.AdditionalTextsProvider
            .Where(static file =>
            {
                string fileName = System.IO.Path.GetFileName(file.Path);
                return string.Equals(fileName, "bgraster-config.schema.json", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(fileName, "bgraster-common.schema.json", StringComparison.OrdinalIgnoreCase);
            });

        IncrementalValueProvider<ImmutableArray<AdditionalText>> collectedFiles = schemaFiles.Collect();

        context.RegisterSourceOutput(collectedFiles, static (productionContext, files) =>
        {
            Generate(productionContext, files);
        });
    }

    static void Generate(SourceProductionContext context, ImmutableArray<AdditionalText> files)
    {
        if (files.IsDefaultOrEmpty)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSchemaDiagnostic, Location.None));
            context.AddSource("GeneratedCliOptionCatalog.g.cs", SourceText.From(BuildCatalogSource([]), Encoding.UTF8));
            return;
        }

        AdditionalText? configSchemaFile = files.FirstOrDefault(static file =>
            string.Equals(System.IO.Path.GetFileName(file.Path), "bgraster-config.schema.json", StringComparison.OrdinalIgnoreCase));

        if (configSchemaFile is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSchemaDiagnostic, Location.None));
            context.AddSource("GeneratedCliOptionCatalog.g.cs", SourceText.From(BuildCatalogSource([]), Encoding.UTF8));
            return;
        }

        SourceText? text = configSchemaFile.GetText(context.CancellationToken);
        if (text is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidSchemaDiagnostic, Location.None, "schema file was empty"));
            context.AddSource("GeneratedCliOptionCatalog.g.cs", SourceText.From(BuildCatalogSource([]), Encoding.UTF8));
            return;
        }

        try
        {
            string? commonSchemaJson = ReadAdditionalFile(files, "bgraster-common.schema.json", context.CancellationToken);
            List<OptionMetadata> options = ParseOptions(text.ToString(), commonSchemaJson);
            context.AddSource("GeneratedCliOptionCatalog.g.cs", SourceText.From(BuildCatalogSource(options), Encoding.UTF8));
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidSchemaDiagnostic, Location.None, ex.Message));
            context.AddSource("GeneratedCliOptionCatalog.g.cs", SourceText.From(BuildCatalogSource([]), Encoding.UTF8));
        }
    }

    static List<OptionMetadata> ParseOptions(string configJson, string? commonSchemaJson)
    {
        using JsonDocument configDocument = JsonDocument.Parse(configJson);
        JsonElement configRoot = configDocument.RootElement;

        JsonDocument? commonDocument = null;
        JsonElement? commonRoot = null;
        if (!string.IsNullOrWhiteSpace(commonSchemaJson))
        {
            string commonSchemaText = commonSchemaJson!;
            commonDocument = JsonDocument.Parse(commonSchemaText);
            commonRoot = commonDocument.RootElement;
        }

        if (!configRoot.TryGetProperty("x-bgraster", out JsonElement metadata) || metadata.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Missing object property 'x-bgraster'.");
        }

        List<OptionMetadata> options = [];
        AppendOptionArray(metadata, "cliOnlyOptions", options, configRoot, commonRoot);
        AppendOptionArray(metadata, "cliOptions", options, configRoot, commonRoot);

        commonDocument?.Dispose();
        return options;
    }

    static string? ReadAdditionalFile(ImmutableArray<AdditionalText> files, string fileName, CancellationToken cancellationToken)
    {
        AdditionalText? file = files.FirstOrDefault(candidate =>
            string.Equals(System.IO.Path.GetFileName(candidate.Path), fileName, StringComparison.OrdinalIgnoreCase));

        if (file is null)
        {
            return null;
        }

        SourceText? text = file.GetText(cancellationToken);
        return text?.ToString();
    }

    static void AppendOptionArray(
        JsonElement metadata,
        string propertyName,
        List<OptionMetadata> destination,
        JsonElement configRoot,
        JsonElement? commonRoot)
    {
        if (!metadata.TryGetProperty(propertyName, out JsonElement arrayElement))
        {
            return;
        }

        if (arrayElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"Property '{propertyName}' must be an array.");
        }

        foreach (JsonElement optionElement in arrayElement.EnumerateArray())
        {
            string? alias = ReadString(optionElement, "alias");
            if (string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            string aliasValue = alias!;
            string? tomlPath = ReadString(optionElement, "tomlPath");
            string description = ReadString(optionElement, "description") ?? "-";
            string enumSuffix = BuildEnumDescriptionSuffix(tomlPath, configRoot, commonRoot);

            destination.Add(new OptionMetadata(
                aliasValue,
                ReadString(optionElement, "valueSyntax"),
                ReadString(optionElement, "typeName") ?? "-",
                ReadString(optionElement, "tomlEquivalent") ?? (tomlPath ?? "-"),
                string.Concat(description, enumSuffix),
                ReadString(optionElement, "defaultResolution") ?? "-"));
        }
    }

    static string BuildEnumDescriptionSuffix(string? tomlPath, JsonElement configRoot, JsonElement? commonRoot)
    {
        if (string.IsNullOrWhiteSpace(tomlPath) || string.Equals(tomlPath, "-", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        if (!TryResolveTomlPathNode(tomlPath!, configRoot, commonRoot, out JsonElement node))
        {
            return string.Empty;
        }

        List<string> values = GetEnumValues(node);
        if (values.Count == 0)
        {
            return string.Empty;
        }

        IEnumerable<string> formatted = values.Distinct().Select(FormatEnumValue);
        return string.Concat(" Allowed values: ", string.Join(", ", formatted), ".");
    }

    static bool TryResolveTomlPathNode(string tomlPath, JsonElement configRoot, JsonElement? commonRoot, out JsonElement node)
    {
        string normalized = tomlPath.Replace("[", string.Empty)
            .Replace("]", string.Empty);

        string[] segments = normalized
            .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        node = configRoot;
        if (segments.Length == 0)
        {
            return false;
        }

        foreach (string rawSegment in segments)
        {
            string segment = rawSegment.Trim();
            if (segment.Length == 0)
            {
                continue;
            }

            if (!TryResolveRefNode(node, configRoot, commonRoot, out JsonElement resolvedCurrent))
            {
                resolvedCurrent = node;
            }

            if (!resolvedCurrent.TryGetProperty("properties", out JsonElement properties)
                || properties.ValueKind != JsonValueKind.Object
                || !properties.TryGetProperty(segment, out JsonElement next))
            {
                return false;
            }

            node = next;
        }

        if (TryResolveRefNode(node, configRoot, commonRoot, out JsonElement resolved))
        {
            node = resolved;
        }

        return true;
    }

    static bool TryResolveRefNode(JsonElement candidate, JsonElement configRoot, JsonElement? commonRoot, out JsonElement resolved)
    {
        resolved = candidate;
        if (!candidate.TryGetProperty("$ref", out JsonElement refElement) || refElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string? reference = refElement.GetString();
        if (string.IsNullOrWhiteSpace(reference))
        {
            return false;
        }

        string referenceText = reference!;

        if (referenceText.StartsWith("./bgraster-common.schema.json#", StringComparison.Ordinal))
        {
            if (!commonRoot.HasValue)
            {
                return false;
            }

            string pointer = referenceText.Substring("./bgraster-common.schema.json".Length);
            JsonElement commonValue = commonRoot.Value;
            return TryGetByPointer(commonValue, pointer, out resolved);
        }

        if (referenceText.StartsWith("#", StringComparison.Ordinal))
        {
            if (TryGetByPointer(configRoot, referenceText, out resolved))
            {
                return true;
            }

            if (commonRoot.HasValue)
            {
                JsonElement commonValue = commonRoot.Value;
                if (TryGetByPointer(commonValue, referenceText, out resolved))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static bool TryGetByPointer(JsonElement root, string pointer, out JsonElement result)
    {
        result = root;
        if (string.IsNullOrWhiteSpace(pointer) || string.Equals(pointer, "#", StringComparison.Ordinal))
        {
            return true;
        }

        if (!pointer.StartsWith("#", StringComparison.Ordinal))
        {
            return false;
        }

        string path = pointer.Substring(1);
        if (path.StartsWith("/", StringComparison.Ordinal))
        {
            path = path.Substring(1);
        }

        if (path.Length == 0)
        {
            return true;
        }

        string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        JsonElement current = root;
        foreach (string rawSegment in segments)
        {
            string segment = rawSegment.Replace("~1", "/").Replace("~0", "~");
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!current.TryGetProperty(segment, out JsonElement next))
                {
                    return false;
                }

                current = next;
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                if (!int.TryParse(segment, NumberStyles.None, CultureInfo.InvariantCulture, out int index))
                {
                    return false;
                }

                JsonElement.ArrayEnumerator enumerator = current.EnumerateArray();
                int currentIndex = 0;
                bool found = false;
                while (enumerator.MoveNext())
                {
                    if (currentIndex == index)
                    {
                        current = enumerator.Current;
                        found = true;
                        break;
                    }

                    currentIndex++;
                }

                if (!found)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        result = current;
        return true;
    }

    static List<string> GetEnumValues(JsonElement node)
    {
        List<string> values = [];

        if (node.TryGetProperty("enum", out JsonElement enumNode) && enumNode.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement value in enumNode.EnumerateArray())
            {
                values.Add(GetEnumValueText(value));
            }

            return values;
        }

        if (node.TryGetProperty("type", out JsonElement typeNode)
            && typeNode.ValueKind == JsonValueKind.String
            && string.Equals(typeNode.GetString(), "array", StringComparison.Ordinal)
            && node.TryGetProperty("items", out JsonElement itemsNode)
            && itemsNode.ValueKind == JsonValueKind.Object
            && itemsNode.TryGetProperty("enum", out JsonElement itemEnum)
            && itemEnum.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement value in itemEnum.EnumerateArray())
            {
                values.Add(GetEnumValueText(value));
            }

            return values;
        }

        if (node.TryGetProperty("oneOf", out JsonElement oneOfNode) && oneOfNode.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement candidate in oneOfNode.EnumerateArray())
            {
                if (candidate.ValueKind == JsonValueKind.Object
                    && candidate.TryGetProperty("enum", out JsonElement candidateEnum)
                    && candidateEnum.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement value in candidateEnum.EnumerateArray())
                    {
                        values.Add(GetEnumValueText(value));
                    }
                }
            }
        }

        return values;
    }

    static string GetEnumValueText(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            _ => value.ToString(),
        };
    }

    static string FormatEnumValue(string value)
    {
        return string.Concat("`", value, "`");
    }

    static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Null => null,
            _ => value.ToString(),
        };
    }

    static string BuildCatalogSource(List<OptionMetadata> options)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("namespace GameshowPro.BgRaster.Configuration;");
        sb.AppendLine();
        sb.AppendLine("static class GeneratedCliOptionCatalog");
        sb.AppendLine("{");
        sb.AppendLine("    internal static ImmutableArray<CliOptionDefinition> Definitions { get; } =");
        sb.AppendLine("    [");

        foreach (OptionMetadata option in options)
        {
            sb.Append("        new(");
            sb.Append(Escape(option.Alias));
            sb.Append(", ");
            sb.Append(option.ValueSyntax is null ? "null" : Escape(option.ValueSyntax));
            sb.Append(", ");
            sb.Append(Escape(option.TypeName));
            sb.Append(", ");
            sb.Append(Escape(option.TomlEquivalent));
            sb.Append(", ");
            sb.Append(Escape(option.Description));
            sb.Append(", ");
            sb.Append(Escape(option.DefaultResolution));
            sb.AppendLine("),");
        }

        sb.AppendLine("    ];");
        sb.AppendLine("}");
        return sb.ToString();
    }

    static string Escape(string value)
    {
        string escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");

        return string.Concat("\"", escaped, "\"");
    }

    sealed class OptionMetadata
    {
        internal OptionMetadata(
            string alias,
            string? valueSyntax,
            string typeName,
            string tomlEquivalent,
            string description,
            string defaultResolution)
        {
            Alias = alias;
            ValueSyntax = valueSyntax;
            TypeName = typeName;
            TomlEquivalent = tomlEquivalent;
            Description = description;
            DefaultResolution = defaultResolution;
        }

        internal string Alias { get; }

        internal string? ValueSyntax { get; }

        internal string TypeName { get; }

        internal string TomlEquivalent { get; }

        internal string Description { get; }

        internal string DefaultResolution { get; }
    }
}
