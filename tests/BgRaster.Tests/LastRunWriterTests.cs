namespace GameshowPro.BgRaster.Tests;

public class LastRunWriterTests
{
    [Fact]
    public void BuildEffectiveConfigToml_IncludesRenderForceAndVerbosity()
    {
        GlobalOptions options = new()
        {
            Render = new RenderOptions
            {
                DryRun = true,
                Output = "C:/temp/out",
                ContinueAfterUnchanged = true,
                MinimumLogLevel = LogLevel.Debug,
            },
        };

        string toml = GameshowPro.BgRaster.StateCache.LastRunWriter.BuildEffectiveConfigToml(options);

        toml.Should().Contain("[render]");
        toml.Should().Contain("no-assignment = true");
        toml.Should().Contain("outputs-skip-unspecified = false");
        toml.Should().Contain("output = \"C:/temp/out\"");
        toml.Should().Contain("force = true");
        toml.Should().Contain("verbosity = \"verbose\"");
    }
}
