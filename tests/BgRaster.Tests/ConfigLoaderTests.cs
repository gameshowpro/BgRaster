namespace GameshowPro.BgRaster.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void ApplyCliOverlay_TextSize_AcceptsStringArrayLiteral()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { TextSize = "[\"3vh\",\"2vh\",\"2vh\"]" };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Text.Size.Should().Equal(["3vh", "2vh", "2vh"]);
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

}
