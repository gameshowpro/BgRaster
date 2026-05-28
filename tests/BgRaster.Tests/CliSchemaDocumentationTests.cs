namespace GameshowPro.BgRaster.Tests;

public class CliSchemaDocumentationTests
{
    [Fact]
    public void CliSchema_OptionsTable_MatchesCatalog()
    {
        string cliSchemaPath = FindCliSchemaPath();
        string markdown = File.ReadAllText(cliSchemaPath);

        int beginIndex = markdown.IndexOf(CliOptionCatalog.OptionsTableBeginMarker, StringComparison.Ordinal);
        int endIndex = markdown.IndexOf(CliOptionCatalog.OptionsTableEndMarker, StringComparison.Ordinal);

        beginIndex.Should().BeGreaterOrEqualTo(0);
        endIndex.Should().BeGreaterThan(beginIndex);

        int contentStart = beginIndex + CliOptionCatalog.OptionsTableBeginMarker.Length;
        string actualTable = markdown[contentStart..endIndex].Trim();
        string expectedTable = CliOptionCatalog.BuildOptionsMarkdownTable().Trim();

        NormalizeLineEndings(actualTable).Should().Be(NormalizeLineEndings(expectedTable));
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

    static string NormalizeLineEndings(string input) =>
        input.Replace("\r\n", "\n", StringComparison.Ordinal);
}