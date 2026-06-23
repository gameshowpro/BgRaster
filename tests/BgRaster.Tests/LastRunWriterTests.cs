// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

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
                NoDiscovery = true,
                Output = "C:/temp/out",
                ContinueAfterUnchanged = true,
                MinimumLogLevel = LogLevel.Debug,
            },
        };

        string toml = GameshowPro.BgRaster.StateCache.LastRunWriter.BuildEffectiveConfigToml(options);

        toml.Should().Contain("[render]");
        toml.Should().Contain("no-assignment = true");
        toml.Should().Contain("no-discovery = true");
        toml.Should().Contain("outputs-skip-unspecified = false");
        toml.Should().Contain("output = \"C:/temp/out\"");
        toml.Should().Contain("force = true");
        toml.Should().Contain("verbosity = \"verbose\"");
    }

    [Fact]
    public void Write_SliceOverrides_ProducesReadableLastRunToml()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-lastrun-{Guid.NewGuid():N}.toml");
        try
        {
            LastRunState state = new()
            {
                Meta = new LastRunMeta
                {
                    Version = "1.0.0",
                    SettingsHash = "hash",
                    Timestamp = "2026-01-01T00:00:00.0000000Z",
                },
                EffectiveConfig = new GlobalOptions
                {
                    Outputs =
                    [
                        new OutputOptions
                        {
                            Target = OutputTarget.FromIndex(0),
                            Slices =
                            [
                                new SliceOptions
                                {
                                    Text = new TextOverride
                                    {
                                        Text = ["line"],
                                        X = "10px",
                                    },
                                    Circle = new CircleOverride
                                    {
                                        X = "0",
                                    },
                                },
                            ],
                        },
                    ],
                },
            };

            GameshowPro.BgRaster.StateCache.LastRunWriter.Write(path, state, "1.0.0");

            string toml = File.ReadAllText(path);
            toml.Should().Contain("[output.slice.text]");
            toml.Should().Contain("[output.slice.circle]");
            GameshowPro.BgRaster.StateCache.LastRunReader.Read(path).Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);

            string tempPath = path + ".tmp";
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void Read_InvalidToml_DeletesUnreadableFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-lastrun-invalid-{Guid.NewGuid():N}.toml");
        try
        {
            File.WriteAllText(path, "[text]\nvalue = 1\n[text]\nvalue = 2\n");

            GameshowPro.BgRaster.StateCache.LastRunReader.Read(path).Should().BeNull();

            File.Exists(path).Should().BeFalse();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
