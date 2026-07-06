// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class CliSchemaDocumentationTests
{
    private const string OptionsTableBeginMarker = "<!-- BEGIN:CLI_OPTIONS_TABLE -->";
    private const string OptionsTableEndMarker = "<!-- END:CLI_OPTIONS_TABLE -->";
    private const string TomlSchemaSectionsBeginMarker = "<!-- BEGIN:TOML_SCHEMA_SECTIONS -->";
    private const string TomlSchemaSectionsEndMarker = "<!-- END:TOML_SCHEMA_SECTIONS -->";

    [Fact]
    public void CliSchema_OptionsTableBlock_UsesSnippetInclude()
    {
        string cliSchemaPath = FindCliSchemaPath();
        string markdown = File.ReadAllText(cliSchemaPath);

        int beginIndex = markdown.IndexOf(OptionsTableBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(OptionsTableEndMarker, StringComparison.Ordinal);

        _ = beginIndex.Should().BeGreaterOrEqualTo(0);
        _ = endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + OptionsTableBeginMarker.Length;
        string includeBlock = markdown[contentStart..endIndex].Trim();

        _ = includeBlock.Should().Be("--8<-- \"generated/cli-schema.md\"");
    }

    [Fact]
    public void TomlSchema_SectionsBlock_UsesSnippetInclude()
    {
        string tomlSchemaPath = FindTomlSchemaPath();
        string markdown = File.ReadAllText(tomlSchemaPath);

        int beginIndex = markdown.IndexOf(TomlSchemaSectionsBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(TomlSchemaSectionsEndMarker, StringComparison.Ordinal);

        _ = beginIndex.Should().BeGreaterOrEqualTo(0);
        _ = endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + TomlSchemaSectionsBeginMarker.Length;
        string includeBlock = markdown[contentStart..endIndex].Trim();

        _ = includeBlock.Should().Be("--8<-- \"generated/toml-schema-sections.md\"");
    }

    [Fact]
    public void ConfigSchema_CliOptionsMetadata_Exists()
    {
        string configSchemaPath = FindConfigSchemaPath();
        string schemaJson = File.ReadAllText(configSchemaPath);
        using JsonDocument document = JsonDocument.Parse(schemaJson);
        JsonElement root = document.RootElement;

        _ = root.TryGetProperty("x-bgraster", out JsonElement metadata).Should().BeTrue();
        _ = metadata.ValueKind.Should().Be(JsonValueKind.Object);

        _ = metadata.TryGetProperty("cliOptions", out JsonElement cliOptions).Should().BeTrue();
        _ = cliOptions.ValueKind.Should().Be(JsonValueKind.Array);
        _ = cliOptions.GetArrayLength().Should().BeGreaterThan(0);

        _ = metadata.TryGetProperty("cliOnlyOptions", out JsonElement cliOnlyOptions).Should().BeTrue();
        _ = cliOnlyOptions.ValueKind.Should().Be(JsonValueKind.Array);

        _ = root.TryGetProperty("x-cli-options", out _).Should().BeFalse();
    }

    private static string FindCliSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "cli-schema.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/cli-schema.md from test runtime directory.");
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

    private static string FindTomlSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "toml-schema.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/toml-schema.md from test runtime directory.");
    }

}