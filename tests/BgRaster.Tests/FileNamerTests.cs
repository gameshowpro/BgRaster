namespace GameshowPro.BgRaster.Tests;

public class FileNamerTests
{
    [Fact]
    public void GetOutputTemplate_Empty_UsesDefaultTemplate()
    {
        string template = FileNamer.GetOutputTemplate(null);

        template.Should().Contain("BgRaster");
        template.Should().Contain("{now}_{index}");
    }

    [Fact]
    public void ResolveRenderOutputPath_ReplacesKnownTokens()
    {
        OutputRecord output = new()
        {
            Index = 2,
            FriendlyName = "Main-Display",
        };

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath("C:\\out\\prefix_{index}_{friendlyName}", output);

        result.FilePath.Should().EndWith("prefix_2_Main-Display.png");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ResolveRenderOutputPath_AppliesSubstitutionTokens()
    {
        OutputRecord output = new()
        {
            Index = 1,
            WidthPx = 1280,
            HeightPx = 720,
            FriendlyName = "Main Display",
        };

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath(
            "C:\\out\\${MachineName}_${OutputName}_${OutputWidth}x${OutputHeight}_{index}",
            output);

        result.FilePath.Should().Contain(Environment.MachineName);
        result.FilePath.Should().Contain("Main Display");
        result.FilePath.Should().Contain("1280x720_1");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ResolveRenderOutputPath_CliRelativeTemplate_UsesCurrentWorkingDirectory_WhenNoConfigIsLoaded()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        GlobalOptions options = ConfigLoader.ApplyCliOverlay(
            new GlobalOptions(),
            new CliOverlay { RenderOutput = "out/wall_{index}" });

        string template = FileNamer.GetOutputTemplate(options.Render.Output);
        OutputRecord output = new() { Index = 4, FriendlyName = "Display" };

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath(template, output);

        result.FilePath.Should().Be(Path.Combine(currentDirectory, "out", "wall_4.png"));
    }

    [Fact]
    public void ResolveRenderOutputPath_UnknownToken_WarnsAndRemovesToken()
    {
        OutputRecord output = new();

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath("C:\\out\\name_{unknown}", output);

        result.FilePath.Should().EndWith("name_.png");
        result.Warnings.Should().ContainSingle();
    }

    [Fact]
    public void IsBgRasterFile_DefaultPatternName_ReturnsTrue()
    {
        string name = "2026-06-03T12-45-01.25_display_0.png";
        FileNamer.IsBgRasterFile(name).Should().BeTrue();
    }

    [Fact]
    public void IsBgRasterFile_ArbitraryPng_ReturnsFalse()
    {
        FileNamer.IsBgRasterFile("wallpaper.png").Should().BeFalse();
    }

    [Fact]
    public void GetOutputDirectory_DefaultTemplate_ContainsBgRaster()
    {
        string dir = FileNamer.GetOutputDirectory(FileNamer.GetOutputTemplate(null));
        dir.Should().Contain("BgRaster");
    }

    [Fact]
    public void GetOutputDirectory_TemplateWithFileStem_ReturnsParentDirectory()
    {
        string dir = FileNamer.GetOutputDirectory("C:\\custom\\path\\stem_{index}");
        dir.Should().Be("C:\\custom\\path");
    }
}
