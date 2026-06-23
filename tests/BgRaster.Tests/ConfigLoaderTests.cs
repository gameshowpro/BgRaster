// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void ApplyCliOverlay_MachineName_OverridesGlobalMachineName()
    {
        GlobalOptions baseOptions = new() { MachineName = "OriginalMachine" };
        CliOverlay overlay = new() { MachineName = "CliMachine" };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.MachineName.Should().Be("CliMachine");
    }

    [Fact]
    public void ApplyCliOverlay_TextSize_AcceptsStringArrayLiteral()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { TextSize = "[\"3vh\",\"2vh\",\"2vh\"]" };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Text.Size.Should().Equal(["3vh", "2vh", "2vh"]);
    }

    [Fact]
    public void ApplyCliOverlay_CircleAndCrosshairPosition_OverridesValues()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new()
        {
            CircleX = "10vw",
            CircleY = "20vh",
            CrosshairX = "30vw",
            CrosshairY = "40vh",
        };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Circle.X.Should().Equal(["10vw"]);
        result.Circle.Y.Should().Equal(["20vh"]);
        result.Crosshair.X.Should().Equal(["30vw"]);
        result.Crosshair.Y.Should().Equal(["40vh"]);
    }

    [Fact]
    public void ApplyCliOverlay_TextSize_AcceptsSingleString()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { TextSize = "2vh" };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Text.Size.Should().Equal(["2vh"]);
    }

    [Fact]
    public void ApplyCliOverlay_LogoOpacity_AcceptsFloatArrayLiteral()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { LogoOpacity = "[0.25, 0.75]" };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Logo.Opacity.Should().Equal([0.25f, 0.75f]);
    }

    [Fact]
    public void ApplyCliOverlay_LogoOpacity_InvalidNumber_ThrowsDescriptiveFormatException()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { LogoOpacity = "not-a-number" };

        Action act = () => ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        act.Should().Throw<FormatException>()
            .WithMessage("*cli --logo-opacity*");
    }

    [Fact]
    public void ApplyCliOverlay_RenderOutputsSkipUnspecified_OverridesRenderOption()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { RenderOutputsSkipUnspecified = true };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Render.OutputsSkipUnspecified.Should().BeTrue();
    }

    [Fact]
    public void ApplyCliOverlay_RenderNoDiscovery_OverridesRenderOption()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { RenderNoDiscovery = true };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Render.NoDiscovery.Should().BeTrue();
    }

    [Fact]
    public void Load_LogoOpacity_ParsesFromTomlAsFloatArray()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[logo]\nopacity = [0.4, 0.8]\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Logo.Opacity.Should().Equal([0.4f, 0.8f]);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_CircleAndCrosshairPosition_ParsesFromToml()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path,
                "[circle]\n" +
                "x = [\"10vw\"]\n" +
                "y = [\"20vh\"]\n" +
                "[crosshair]\n" +
                "x = [\"30vw\"]\n" +
                "y = [\"40vh\"]\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Circle.X.Should().Equal(["10vw"]);
            result.Circle.Y.Should().Equal(["20vh"]);
            result.Crosshair.X.Should().Equal(["30vw"]);
            result.Crosshair.Y.Should().Equal(["40vh"]);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_MachineName_ParsesFromToml()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "machine-name = \"TomlMachine\"\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.MachineName.Should().Be("TomlMachine");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_LogoOpacity_StringInToml_ThrowsDescriptiveFormatException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[logo]\nopacity = [\"0.5\"]\n");

            Action act = () => ConfigLoader.Load(path);

            act.Should().Throw<FormatException>()
                .WithMessage("*config [logo].opacity*");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_RenderOutputsSkipUnspecified_ParsesFromToml()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[render]\noutputs-skip-unspecified = true\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Render.OutputsSkipUnspecified.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_RenderNoDiscovery_ParsesFromToml()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[render]\nno-discovery = true\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Render.NoDiscovery.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_OutputHardwareOutput_ParsesNestedTable()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[[output]]\ntarget = 0\n[output.hardware_output]\nwidthPx = 800\nheightPx = 600\nfriendlyName = \"Fixture\"\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Outputs.Should().HaveCount(1);
            result.Outputs[0].HardwareOutput.Should().NotBeNull();
            result.Outputs[0].HardwareOutput!.WidthPx.Should().Be(800);
            result.Outputs[0].HardwareOutput!.HeightPx.Should().Be(600);
            result.Outputs[0].HardwareOutput!.FriendlyName.Should().Be("Fixture");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_RelativePaths_AreResolvedAgainstConfigDirectory()
    {
        string configDir = Path.Combine(Path.GetTempPath(), $"bgraster-config-dir-{Guid.NewGuid():N}");
        string path = Path.Combine(configDir, "config.toml");

        Directory.CreateDirectory(configDir);
        try
        {
            File.WriteAllText(path,
                "[background]\n" +
                "image = [\"images/bg.png\"]\n" +
                "[logo]\n" +
                "source = [\"logos/mark.svg\"]\n" +
                "[render]\n" +
                "output = \"output/{index}\"\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Background.Image.Should().Equal([Path.GetFullPath(Path.Combine(configDir, "images", "bg.png"))]);
            result.Logo.Source.Should().Equal([Path.GetFullPath(Path.Combine(configDir, "logos", "mark.svg"))]);
            result.Render.Output.Should().Be(Path.GetFullPath(Path.Combine(configDir, "output", "{index}")));
        }
        finally
        {
            if (Directory.Exists(configDir))
                Directory.Delete(configDir, recursive: true);
        }
    }

    [Fact]
    public void Load_OutputAndSliceRelativePaths_AreResolvedAgainstConfigDirectory()
    {
        string configDir = Path.Combine(Path.GetTempPath(), $"bgraster-config-dir-{Guid.NewGuid():N}");
        string path = Path.Combine(configDir, "config.toml");

        Directory.CreateDirectory(configDir);
        try
        {
            File.WriteAllText(path,
                "[[output]]\n" +
                "target = 0\n" +
                "background = { image = \"output-bg.png\" }\n" +
                "logo = { source = \"output-logo.svg\" }\n" +
                "[[output.slice]]\n" +
                "x = \"0\"\n" +
                "y = \"0\"\n" +
                "width = \"100vw\"\n" +
                "height = \"100vh\"\n" +
                "background = { image = \"slice-bg.png\" }\n" +
                "logo = { source = \"slice-logo.svg\" }\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Outputs.Should().HaveCount(1);
            result.Outputs[0].Background.Should().NotBeNull();
            result.Outputs[0].Background!.Image.Should().Be(Path.GetFullPath(Path.Combine(configDir, "output-bg.png")));
            result.Outputs[0].Logo.Should().NotBeNull();
            result.Outputs[0].Logo!.Source.Should().Be(Path.GetFullPath(Path.Combine(configDir, "output-logo.svg")));

            result.Outputs[0].Slices.Should().HaveCount(1);
            result.Outputs[0].Slices[0].Background.Should().NotBeNull();
            result.Outputs[0].Slices[0].Background!.Image.Should().Be(Path.GetFullPath(Path.Combine(configDir, "slice-bg.png")));
            result.Outputs[0].Slices[0].Logo.Should().NotBeNull();
            result.Outputs[0].Slices[0].Logo!.Source.Should().Be(Path.GetFullPath(Path.Combine(configDir, "slice-logo.svg")));
        }
        finally
        {
            if (Directory.Exists(configDir))
                Directory.Delete(configDir, recursive: true);
        }
    }

    [Theory]
    [InlineData("force")]
    [InlineData("render-force")]
    public void Load_RenderForce_AcceptsCanonicalAndLegacyKey(string key)
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, $"[render]\n{key} = true\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.Render.ContinueAfterUnchanged.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_InvalidToml_ThrowsFormatExceptionWithConfigPath()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[text\ntext = [\"oops\"]\n");

            Action act = () => ConfigLoader.Load(path);

            act.Should().Throw<FormatException>()
                .WithMessage($"*{path}*");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_LabeledEdges_SideRejectsDuplicates()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[labeled-edges]\nside = [\"TL\", \"TL\"]\n");

            Action act = () => ConfigLoader.Load(path);

            act.Should().Throw<FormatException>()
                .WithMessage("*Duplicate labeled-edge side*");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_LabeledEdges_ParsesTailLengthAndScope()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[labeled-edges]\ntail-length = [\"6px\"]\nscope = [\"Slice\"]\n");

            GlobalOptions result = ConfigLoader.Load(path);

            result.LabeledEdges.TailLength.Should().Equal(["6px"]);
            result.LabeledEdges.Scope.Should().Equal([LabeledEdgesScope.Slice]);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_LabeledEdges_ScopeRejectsLegacySystemValue()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-config-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[labeled-edges]\nscope = [\"System\"]\n");

            Action act = () => ConfigLoader.Load(path);

            act.Should().Throw<FormatException>()
                .WithMessage("*expected one of Desktop, Output, Slice*");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

}
