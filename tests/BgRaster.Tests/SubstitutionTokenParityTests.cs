// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public sealed class SubstitutionTokenParityTests
{
    [Fact]
    public void Schema_SubstitutionTokens_AreHandledByFieldSubstitutor()
    {
        string schemaPath = FindConfigSchemaPath();
        string schemaJson = File.ReadAllText(schemaPath);

        using JsonDocument document = JsonDocument.Parse(schemaJson);
        JsonElement root = document.RootElement;

        root.TryGetProperty("x-bgraster", out JsonElement metadata).Should().BeTrue();
        metadata.TryGetProperty("tomlReference", out JsonElement tomlRef).Should().BeTrue();
        tomlRef.TryGetProperty("appendix", out JsonElement appendix).Should().BeTrue();
        appendix.TryGetProperty("substitutionTokens", out JsonElement tokens).Should().BeTrue();
        tokens.ValueKind.Should().Be(JsonValueKind.Array);

        List<string> expectedTokens = [];
        foreach (JsonElement token in tokens.EnumerateArray())
        {
            if (TryGetNonEmptyProperty(token, "machineScoped", out string? m)) expectedTokens.Add(m!);
            if (TryGetNonEmptyProperty(token, "outputScoped", out string? o)) expectedTokens.Add(o!);
            if (TryGetNonEmptyProperty(token, "sliceScoped", out string? s)) expectedTokens.Add(s!);
        }

        expectedTokens.Should().NotBeEmpty("schema substitutionTokens array must not be empty.");

        SubstitutionContext ctx = new(
            MachineName: "TestPC",
            OutputWidth: 1920,
            OutputHeight: 1080,
            OutputIndex: 0,
            OutputName: "Display1",
            SliceWidth: 640,
            SliceHeight: 480,
            SliceIndex: 0);

        foreach (string token in expectedTokens)
        {
            string result = FieldSubstitutor.Substitute(token, ctx);
            result.Should().NotBe(token, $"{token} must be substituted (not pass through unchanged).");
        }
    }

    [Fact]
    public void Schema_SubstitutionTokens_AreIncludedInGeneratedCatalog()
    {
        string schemaPath = FindConfigSchemaPath();
        string schemaJson = File.ReadAllText(schemaPath);

        using JsonDocument document = JsonDocument.Parse(schemaJson);
        JsonElement root = document.RootElement;

        root.TryGetProperty("x-bgraster", out JsonElement metadata).Should().BeTrue();
        metadata.TryGetProperty("tomlReference", out JsonElement tomlRef).Should().BeTrue();
        tomlRef.TryGetProperty("appendix", out JsonElement appendix).Should().BeTrue();
        appendix.TryGetProperty("substitutionTokens", out JsonElement tokens).Should().BeTrue();

        foreach (JsonElement token in tokens.EnumerateArray())
        {
            token.TryGetProperty("machineScoped", out JsonElement machineElement).Should().BeTrue();
            token.TryGetProperty("outputScoped", out JsonElement outputElement).Should().BeTrue();
            token.TryGetProperty("sliceScoped", out JsonElement sliceElement).Should().BeTrue();
        }
    }

    static bool TryGetNonEmptyProperty(JsonElement element, string propertyName, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out JsonElement prop))
            return false;
        string? s = prop.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return false;
        value = s;
        return true;
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

        throw new InvalidOperationException("Could not locate docs/schemas/bgraster-config.schema.json");
    }
}
