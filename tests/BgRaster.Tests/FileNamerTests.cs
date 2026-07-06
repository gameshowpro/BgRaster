// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class FileNamerTests
{
    [Fact]
    public void GetOutputTemplate_Empty_UsesDefaultTemplate()
    {
        string template = FileNamer.GetOutputTemplate(null);

        _ = template.Should().Contain("BgRaster");
        _ = template.Should().Contain("{now}_{index}");
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

        _ = result.FilePath.Should().EndWith("prefix_2_Main-Display.png");
        _ = result.Warnings.Should().BeEmpty();
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

        _ = result.FilePath.Should().Contain(Environment.MachineName);
        _ = result.FilePath.Should().Contain("Main Display");
        _ = result.FilePath.Should().Contain("1280x720_1");
        _ = result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ResolveRenderOutputPath_UsesConfiguredMachineNameOverride()
    {
        OutputRecord output = new()
        {
            Index = 1,
            WidthPx = 1280,
            HeightPx = 720,
            FriendlyName = "Main Display",
        };

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath(
            "C:\\out\\${MachineName}_${OutputName}_{index}",
            output,
            configuredMachineName: "ConfiguredMachine");

        _ = result.FilePath.Should().Contain("ConfiguredMachine");
        _ = result.Warnings.Should().BeEmpty();
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

        _ = result.FilePath.Should().Be(Path.Combine(currentDirectory, "out", "wall_4.png"));
    }

    [Fact]
    public void ResolveRenderOutputPath_UnknownToken_WarnsAndRemovesToken()
    {
        OutputRecord output = new();

        FileNamer.RenderOutputPathResult result = FileNamer.ResolveRenderOutputPath("C:\\out\\name_{unknown}", output);

        _ = result.FilePath.Should().EndWith("name_.png");
        _ = result.Warnings.Should().ContainSingle();
    }

    [Fact]
    public void IsBgRasterFile_DefaultPatternName_ReturnsTrue()
    {
        string name = "2026-06-03T12-45-01.25_display_0.png";
        _ = FileNamer.IsBgRasterFile(name).Should().BeTrue();
    }

    [Fact]
    public void IsBgRasterFile_ArbitraryPng_ReturnsFalse()
    {
        _ = FileNamer.IsBgRasterFile("wallpaper.png").Should().BeFalse();
    }

    [Fact]
    public void GetOutputDirectory_DefaultTemplate_ContainsBgRaster()
    {
        string dir = FileNamer.GetOutputDirectory(FileNamer.GetOutputTemplate(null));
        _ = dir.Should().Contain("BgRaster");
    }

    [Fact]
    public void GetOutputDirectory_TemplateWithFileStem_ReturnsParentDirectory()
    {
        string dir = FileNamer.GetOutputDirectory("C:\\custom\\path\\stem_{index}");
        _ = dir.Should().Be("C:\\custom\\path");
    }
}
