// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.Text.Json;

namespace GameshowPro.BgRaster.Tests;

public class CliSchemaDocumentationTests
{
    const string OptionsTableBeginMarker = "<!-- BEGIN:CLI_OPTIONS_TABLE -->";
    const string OptionsTableEndMarker = "<!-- END:CLI_OPTIONS_TABLE -->";
    const string TomlRootScalarsBeginMarker = "<!-- BEGIN:TOML_ROOT_SCALARS_TABLE -->";
    const string TomlRootScalarsEndMarker = "<!-- END:TOML_ROOT_SCALARS_TABLE -->";
    const string TomlSchemaSectionsBeginMarker = "<!-- BEGIN:TOML_SCHEMA_SECTIONS -->";
    const string TomlSchemaSectionsEndMarker = "<!-- END:TOML_SCHEMA_SECTIONS -->";

    [Fact]
    public void CliSchema_OptionsTableBlock_UsesSnippetInclude()
    {
        string cliSchemaPath = FindCliSchemaPath();
        string markdown = File.ReadAllText(cliSchemaPath);

        int beginIndex = markdown.IndexOf(OptionsTableBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(OptionsTableEndMarker, StringComparison.Ordinal);

        beginIndex.Should().BeGreaterOrEqualTo(0);
        endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + OptionsTableBeginMarker.Length;
        string includeBlock = markdown[contentStart..endIndex].Trim();

        includeBlock.Should().Be("--8<-- \"generated/cli-schema.md\"");
    }

    [Fact]
    public void TomlSchema_RootScalarsBlock_IsEmptyInSource()
    {
        string tomlSchemaPath = FindTomlSchemaPath();
        string markdown = File.ReadAllText(tomlSchemaPath);

        int beginIndex = markdown.IndexOf(TomlRootScalarsBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(TomlRootScalarsEndMarker, StringComparison.Ordinal);

        beginIndex.Should().BeGreaterOrEqualTo(0);
        endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + TomlRootScalarsBeginMarker.Length;
        string includeBlock = markdown[contentStart..endIndex].Trim();

        includeBlock.Should().Be("--8<-- \"generated/toml-root-scalars.md\"");
    }

    [Fact]
    public void TomlSchema_SectionsBlock_UsesSnippetInclude()
    {
        string tomlSchemaPath = FindTomlSchemaPath();
        string markdown = File.ReadAllText(tomlSchemaPath);

        int beginIndex = markdown.IndexOf(TomlSchemaSectionsBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(TomlSchemaSectionsEndMarker, StringComparison.Ordinal);

        beginIndex.Should().BeGreaterOrEqualTo(0);
        endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + TomlSchemaSectionsBeginMarker.Length;
        string includeBlock = markdown[contentStart..endIndex].Trim();

        includeBlock.Should().Be("--8<-- \"generated/toml-schema-sections.md\"");
    }

    [Fact]
    public void ConfigSchema_CliOptionsMetadata_Exists()
    {
        string configSchemaPath = FindConfigSchemaPath();
        string schemaJson = File.ReadAllText(configSchemaPath);
        using JsonDocument document = JsonDocument.Parse(schemaJson);
        JsonElement root = document.RootElement;

        root.TryGetProperty("x-bgraster", out JsonElement metadata).Should().BeTrue();
        metadata.ValueKind.Should().Be(JsonValueKind.Object);

        metadata.TryGetProperty("cliOptions", out JsonElement cliOptions).Should().BeTrue();
        cliOptions.ValueKind.Should().Be(JsonValueKind.Array);
        cliOptions.GetArrayLength().Should().BeGreaterThan(0);

        metadata.TryGetProperty("cliOnlyOptions", out JsonElement cliOnlyOptions).Should().BeTrue();
        cliOnlyOptions.ValueKind.Should().Be(JsonValueKind.Array);

        root.TryGetProperty("x-cli-options", out JsonElement _).Should().BeFalse();
    }

    static string FindCliSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "cli-schema.md");
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/cli-schema.md from test runtime directory.");
    }

    static string FindConfigSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "schemas", "bgraster-config.schema.json");
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/schemas/bgraster-config.schema.json from test runtime directory.");
    }

    static string FindTomlSchemaPath()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "docs", "toml-schema.md");
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/toml-schema.md from test runtime directory.");
    }

}