// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GameshowPro.BgRaster.SchemaMetadataGenerator;

[Generator]
public sealed class DefaultsGenerator : IIncrementalGenerator
{
    static readonly DiagnosticDescriptor MissingSchemaDiagnostic = new(
        id: "GSPGEN010",
        title: "Common schema not found for defaults generation",
        messageFormat: "Could not locate bgraster-common.schema.json in AdditionalFiles. Defaults will not be generated.",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor ParseFailureDiagnostic = new(
        id: "GSPGEN011",
        title: "Could not parse common schema for defaults generation",
        messageFormat: "Could not parse bgraster-common.schema.json: {0}",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor MissingDefaultDiagnostic = new(
        id: "GSPGEN012",
        title: "Schema property is missing a default value",
        messageFormat: "Property '{0}' in definition '{1}' has no default value. All properties in definitions used for runtime defaults must have a default.",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor UnknownTypeDiagnostic = new(
        id: "GSPGEN013",
        title: "Schema property has an unsupported type",
        messageFormat: "Property '{0}' in definition '{1}' has unsupported type: {2}",
        category: "BgRaster.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Definitions that map to runtime Options records (which have defaults).
    // Override types are skipped — they have no defaults (all nullable).
    // outputList and hardwareOutput are skipped — structurally different.
    static readonly ImmutableArray<string> DefaultableDefinitions =
    [
        "textTable",
        "backgroundTable",
        "gridTable",
        "circleTable",
        "crosshairTable",
        "labeledEdgesTable",
        "logoTable",
        "networkTable",
        "renderTable",
    ];

    // Schema definition name → generated C# class name
    static readonly ImmutableDictionary<string, string> DefinitionToClass = new Dictionary<string, string>
    {
        ["textTable"] = "TextDefaults",
        ["backgroundTable"] = "BackgroundDefaults",
        ["gridTable"] = "GridDefaults",
        ["circleTable"] = "CircleDefaults",
        ["crosshairTable"] = "CrosshairDefaults",
        ["labeledEdgesTable"] = "LabeledEdgesDefaults",
        ["logoTable"] = "LogoDefaults",
        ["networkTable"] = "NetworkDefaults",
        ["renderTable"] = "RenderDefaults",
    }.ToImmutableDictionary();

    // Properties whose schema string values map to C# enum types.
    static readonly ImmutableDictionary<string, string> EnumPropertyTypes = new Dictionary<string, string>
    {
        ["labeledEdgesTable.scope"] = "LabeledEdgesScope",
        ["labeledEdgesTable.side"] = "LabeledEdgeSide",
    }.ToImmutableDictionary();

    static string KebabToPascal(string name)
    {
        StringBuilder sb = new();
        bool upper = true;
        foreach (char c in name)
        {
            if (c is '-' or '_')
            {
                upper = true;
                continue;
            }
            sb.Append(upper ? Char.ToUpperInvariant(c) : c);
            upper = false;
        }
        return sb.ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> commonSchemaFiles = context.AdditionalTextsProvider
            .Where(static file =>
                string.Equals(System.IO.Path.GetFileName(file.Path),
                    "bgraster-common.schema.json", StringComparison.OrdinalIgnoreCase));

        IncrementalValueProvider<ImmutableArray<AdditionalText>> collected = commonSchemaFiles.Collect();

        context.RegisterSourceOutput(collected, static (productionContext, files) =>
        {
            Generate(productionContext, files);
        });
    }

    static void Generate(SourceProductionContext context, ImmutableArray<AdditionalText> files)
    {
        AdditionalText? commonFile = files.FirstOrDefault(static f =>
            string.Equals(System.IO.Path.GetFileName(f.Path),
                "bgraster-common.schema.json", StringComparison.OrdinalIgnoreCase));

        if (commonFile is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSchemaDiagnostic, Location.None));
            return;
        }

        SourceText? text = commonFile.GetText(context.CancellationToken);
        if (text is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                "schema file was empty"));
            return;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(text.ToString());
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                ex.Message));
            return;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("definitions", out JsonElement definitions) ||
                definitions.ValueKind != JsonValueKind.Object)
            {
                context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                    "missing or invalid 'definitions' object"));
                return;
            }

            List<DefaultsClass> classes = [];
            bool hasErrors = false;

            foreach (string definitionName in DefaultableDefinitions)
            {
                if (!definitions.TryGetProperty(definitionName, out JsonElement defElement) ||
                    defElement.ValueKind != JsonValueKind.Object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                        $"definition '{definitionName}' not found"));
                    hasErrors = true;
                    continue;
                }

                if (!defElement.TryGetProperty("properties", out JsonElement props) ||
                    props.ValueKind != JsonValueKind.Object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                        $"definition '{definitionName}' has no properties"));
                    hasErrors = true;
                    continue;
                }

                string className = DefinitionToClass[definitionName];
                List<DefaultsProperty> properties = [];

                foreach (JsonProperty prop in props.EnumerateObject())
                {
                    string pascalName = KebabToPascal(prop.Name);

                    if (!prop.Value.TryGetProperty("default", out JsonElement defaultElement))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MissingDefaultDiagnostic, Location.None,
                            prop.Name, definitionName));
                        hasErrors = true;
                        continue;
                    }

                    string csharpType = ResolveCSharpType(prop.Value, definitionName, prop.Name, context, ref hasErrors);
                    if (csharpType is null)
                        continue;

                    string? defaultValue = FormatDefaultValue(defaultElement, csharpType, definitionName, prop.Name);
                    if (defaultValue is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ParseFailureDiagnostic, Location.None,
                            $"could not format default value for '{prop.Name}' in '{definitionName}'"));
                        hasErrors = true;
                        continue;
                    }

                    properties.Add(new DefaultsProperty(pascalName, csharpType, defaultValue));
                }

                classes.Add(new DefaultsClass(className, properties.ToImmutableArray()));
            }

            if (hasErrors)
                return;

            string source = BuildDefaultsSource(classes.ToImmutableArray());
            context.AddSource("Defaults.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    static string? ResolveCSharpType(JsonElement propElement, string definitionName, string propName,
        SourceProductionContext context, ref bool hasErrors)
    {
        // Check for enum mapping first
        if (EnumPropertyTypes.TryGetValue($"{definitionName}.{propName}", out string? enumType))
        {
            if (propElement.TryGetProperty("type", out JsonElement typeEl) &&
                typeEl.ValueKind == JsonValueKind.String &&
                typeEl.GetString() == "array")
            {
                return $"ImmutableArray<{enumType}>";
            }
        }

        if (!propElement.TryGetProperty("type", out JsonElement typeElement) ||
            typeElement.ValueKind != JsonValueKind.String)
        {
            // Handle anyOf (used for anchor-x/anchor-y which accept string or number)
            // C# always stores these as string
            if (propElement.TryGetProperty("anyOf", out _))
                return "string";

            context.ReportDiagnostic(Diagnostic.Create(UnknownTypeDiagnostic, Location.None,
                propName, definitionName, "missing type"));
            hasErrors = true;
            return null;
        }

        string schemaType = typeElement.GetString()!;

        return schemaType switch
        {
            "string" => "string",
            "integer" => "int",
            "number" => "float",
            "boolean" => "bool",
            "array" => ResolveArrayItemType(propElement, definitionName, propName, context, ref hasErrors),
            _ => null,
        };
    }

    static string? ResolveArrayItemType(JsonElement propElement, string definitionName, string propName,
        SourceProductionContext context, ref bool hasErrors)
    {
        if (!propElement.TryGetProperty("items", out JsonElement items) ||
            items.ValueKind != JsonValueKind.Object)
        {
            context.ReportDiagnostic(Diagnostic.Create(UnknownTypeDiagnostic, Location.None,
                propName, definitionName, "array without items type"));
            hasErrors = true;
            return null;
        }

        if (!items.TryGetProperty("type", out JsonElement itemType) ||
            itemType.ValueKind != JsonValueKind.String)
        {
            context.ReportDiagnostic(Diagnostic.Create(UnknownTypeDiagnostic, Location.None,
                propName, definitionName, "array items missing type"));
            hasErrors = true;
            return null;
        }

        string typeStr = itemType.GetString()!;
        return typeStr switch
        {
            "string" => "ImmutableArray<string>",
            "number" => "ImmutableArray<float>",
            "boolean" => "ImmutableArray<bool>",
            _ => null,
        };
    }

    static string? FormatDefaultValue(JsonElement defaultElement, string csharpType,
        string definitionName, string propName)
    {
        // Scalar types
        if (csharpType == "string")
        {
            if (defaultElement.ValueKind != JsonValueKind.String)
                return null;
            string val = defaultElement.GetString()!;
            // Schema uses <br> for newlines; convert to actual newline for C#
                        val = val.Replace("<br>", "\n");
            return $"\"{EscapeCSharpString(val)}\"";
        }

        if (csharpType == "int")
        {
            if (defaultElement.ValueKind != JsonValueKind.Number)
                return null;
            return defaultElement.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (csharpType == "float")
        {
            if (defaultElement.ValueKind != JsonValueKind.Number)
                return null;
            return defaultElement.GetDouble().ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "f";
        }

        if (csharpType == "bool")
        {
            if (defaultElement.ValueKind is JsonValueKind.True)
                return "true";
            if (defaultElement.ValueKind is JsonValueKind.False)
                return "false";
            return null;
        }

        // Array types
        if (csharpType.StartsWith("ImmutableArray<", StringComparison.Ordinal))
        {
            if (defaultElement.ValueKind != JsonValueKind.Array)
                return null;

            string innerType = csharpType.Substring("ImmutableArray<".Length, csharpType.Length - "ImmutableArray<".Length - 1);

            if (defaultElement.GetArrayLength() == 0)
                return $"ImmutableArray<{innerType}>.Empty";

            List<string> elements = [];
            foreach (JsonElement item in defaultElement.EnumerateArray())
            {
                string? formatted = FormatScalarValue(item, innerType);
                if (formatted is null)
                    return null;
                elements.Add(formatted);
            }

            return $"ImmutableArray.Create({string.Join(", ", elements)})";
        }

        return null;
    }

    static string? FormatScalarValue(JsonElement element, string innerType)
    {
        return innerType switch
        {
            "string" => $"\"{EscapeCSharpString(element.GetString()!)}\"",
            "float" => element.GetDouble().ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "f",
            "bool" => element.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null,
            },
            "int" => element.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => element.ValueKind == JsonValueKind.String
                ? $"{innerType}.{element.GetString()}"
                : null,
        };
    }

    static string EscapeCSharpString(string s)
    {
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }

    static string BuildDefaultsSource(ImmutableArray<DefaultsClass> classes)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using System.Collections.Immutable;");
        sb.AppendLine("using GameshowPro.BgRaster.Models;");
        sb.AppendLine();
        sb.AppendLine("namespace GameshowPro.BgRaster.Configuration;");
        sb.AppendLine();

        foreach (DefaultsClass cls in classes)
        {
            sb.AppendLine($"static class {cls.Name}");
            sb.AppendLine("{");
            foreach (DefaultsProperty prop in cls.Properties)
            {
                sb.AppendLine($"    internal static {prop.Type} {prop.Name} {{ get; }} = {prop.DefaultValue};");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    sealed class DefaultsProperty
    {
        internal string Name { get; }
        internal string Type { get; }
        internal string DefaultValue { get; }

        internal DefaultsProperty(string name, string type, string defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    sealed class DefaultsClass
    {
        internal string Name { get; }
        internal ImmutableArray<DefaultsProperty> Properties { get; }

        internal DefaultsClass(string name, ImmutableArray<DefaultsProperty> properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}