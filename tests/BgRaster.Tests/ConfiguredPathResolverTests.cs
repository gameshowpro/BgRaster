// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class ConfiguredPathResolverTests
{
    [Fact]
    public void Resolve_LeavesPackUriUnchanged()
    {
        string packUri = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg";

        string resolved = ConfiguredPathResolver.Resolve(packUri, @"C:\base");

        _ = resolved.Should().Be(packUri);
    }

    [Fact]
    public void ApplyCliOverlay_NormalizesPathValuesImmediately()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"bgraster-cli-paths-{Guid.NewGuid():N}");
        string environmentVariableName = "BGRASTER_TEST_CLI_PATH_ROOT";
        string? originalEnvironmentVariable = Environment.GetEnvironmentVariable(environmentVariableName);

        try
        {
            _ = Directory.CreateDirectory(tempRoot);
            Environment.SetEnvironmentVariable(environmentVariableName, tempRoot);

            CliOverlay overlay = new()
            {
                BackgroundImage = $"%{environmentVariableName}%\\backgrounds\\test.png",
                LogoSource = "pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg",
                RenderOutput = $"%{environmentVariableName}%\\output",
            };

            GlobalOptions merged = ConfigLoader.ApplyCliOverlay(new GlobalOptions(), overlay);

            _ = merged.Background.Image[0].Should().Be(Path.GetFullPath(Path.Combine(tempRoot, "backgrounds", "test.png")));
            _ = merged.Logo.Source[0].Should().Be("pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg");
            _ = merged.Render.Output.Should().Be(Path.GetFullPath(Path.Combine(tempRoot, "output")));
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariableName, originalEnvironmentVariable);
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
