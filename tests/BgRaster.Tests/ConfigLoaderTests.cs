namespace GameshowPro.BgRaster.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void ApplyCliOverlay_RenderOutputsSkipUnspecified_OverridesRenderOption()
    {
        GlobalOptions baseOptions = new();
        CliOverlay overlay = new() { RenderOutputsSkipUnspecified = true };

        GlobalOptions result = ConfigLoader.ApplyCliOverlay(baseOptions, overlay);

        result.Render.OutputsSkipUnspecified.Should().BeTrue();
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
}
