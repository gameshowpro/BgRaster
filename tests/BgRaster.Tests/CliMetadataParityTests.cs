// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.CommandLine;

namespace GameshowPro.BgRaster.Tests;

public sealed class CliMetadataParityTests
{
    [Fact]
    public void CliBinding_Options_MatchSchemaMetadataAliases()
    {
        RootCommand root = CliBinding.BuildRootCommand(static (_, _) => Task.FromResult(0));

        string schemaPath = FindConfigSchemaPath();
        string schemaJson = File.ReadAllText(schemaPath);

        using JsonDocument document = JsonDocument.Parse(schemaJson);
        JsonElement rootElement = document.RootElement;

        _ = rootElement.TryGetProperty("x-bgraster", out JsonElement metadata).Should().BeTrue();

        List<(string Alias, string TypeName)> schemaOptions = [];
        AppendOptions(metadata, "cliOnlyOptions", schemaOptions);
        AppendOptions(metadata, "cliOptions", schemaOptions);

        foreach ((string alias, string typeName) in schemaOptions)
        {
            string value = string.Equals(typeName, "bool", StringComparison.Ordinal) ? "true" : "sample";
            ParseResult parseResult = root.Parse($"{alias} {value}");
            _ = parseResult.Errors.Should().BeEmpty($"{alias} should be a bound CLI option.");
        }

        ImmutableArray<string> generatedAliases = [.. GeneratedCliOptionCatalog.Definitions.Select(static definition => definition.Alias)];
        _ = generatedAliases.Should().BeEquivalentTo(schemaOptions.Select(static option => option.Alias));
    }

    private static void AppendOptions(JsonElement metadata, string propertyName, List<(string Alias, string TypeName)> destination)
    {
        _ = metadata.TryGetProperty(propertyName, out JsonElement options).Should().BeTrue();
        _ = options.ValueKind.Should().Be(JsonValueKind.Array);

        foreach (JsonElement option in options.EnumerateArray())
        {
            _ = option.TryGetProperty("alias", out JsonElement aliasElement).Should().BeTrue();
            _ = aliasElement.ValueKind.Should().Be(JsonValueKind.String);
            _ = option.TryGetProperty("typeName", out JsonElement typeNameElement).Should().BeTrue();
            _ = typeNameElement.ValueKind.Should().Be(JsonValueKind.String);

            string? alias = aliasElement.GetString();
            string? typeName = typeNameElement.GetString();
            _ = string.IsNullOrWhiteSpace(alias).Should().BeFalse();
            _ = string.IsNullOrWhiteSpace(typeName).Should().BeFalse();
            destination.Add((alias!, typeName!));
        }
    }

    private static string FindConfigSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "schemas", "bgraster-config.schema.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/schemas/bgraster-config.schema.json from test runtime directory.");
    }
}
