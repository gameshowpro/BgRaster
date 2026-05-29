namespace GameshowPro.BgRaster.Tests;

using GameshowPro.BgRaster;

public class ProgramConfigTests
{
    [Fact]
    public void GetDefaultConfigSearchPaths_ReturnsRequestedHierarchy()
    {
        ImmutableArray<string> paths = Program.GetDefaultConfigSearchPaths(
            executableDirectory: @"C:\Apps\BgRaster",
            commonApplicationDataDirectory: @"C:\ProgramData",
            localApplicationDataDirectory: @"C:\Users\User\AppData\Local",
            applicationDataDirectory: @"C:\Users\User\AppData\Roaming");

        paths.Should().Equal(
            @"C:\Apps\BgRaster\config.toml",
            @"C:\ProgramData\BgInfo\config.toml",
            @"C:\Users\User\AppData\Local\BgInfo\config.toml",
            @"C:\Users\User\AppData\Roaming\BgInfo\config.toml");
    }

    [Fact]
    public void ResolveConfigPath_UsesFirstExistingDefaultCandidate()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            string exeDir = Path.Combine(tempRoot, "exe");
            string programData = Path.Combine(tempRoot, "programData");
            string localAppData = Path.Combine(tempRoot, "localAppData");
            string appData = Path.Combine(tempRoot, "appData");

            Directory.CreateDirectory(exeDir);
            Directory.CreateDirectory(programData);
            Directory.CreateDirectory(localAppData);
            Directory.CreateDirectory(appData);

            ImmutableArray<string> paths = Program.GetDefaultConfigSearchPaths(exeDir, programData, localAppData, appData);
            Directory.CreateDirectory(Path.GetDirectoryName(paths[2])!);
            File.WriteAllText(paths[2], "[render]\nverbosity = \"normal\"\n");

            string resolvedPath = Program.ResolveConfigPath(null, paths);

            resolvedPath.Should().Be(paths[2]);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ShouldSeedExplicitConfigFromDefaults_ReturnsTrueOnlyForMissingExplicitNonDryRunConfig()
    {
        Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false).Should().BeTrue();
        Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: true, isDryRun: false).Should().BeFalse();
        Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: true).Should().BeFalse();
        Program.ShouldSeedExplicitConfigFromDefaults(null, configExistedAtStartup: false, isDryRun: false).Should().BeFalse();
    }

    [Fact]
    public void BuildSeedConfigToml_UsesConfigSchemaAndOutputTargetVariants()
    {
        GlobalOptions options = new();
        ImmutableArray<OutputRecord> outputs =
        [
            new OutputRecord { Id = "\\\\?\\DISPLAY#B#{id}", Index = 1 },
            new OutputRecord { Id = "\\\\?\\DISPLAY#A#{id}", Index = 0 },
        ];

        string toml = Program.BuildSeedConfigToml(options, outputs);

        toml.Should().Contain("bgraster-config.schema.json");
        toml.Should().Contain("[[output]]");
        toml.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#A#{id}\"");
        toml.Should().Contain("# target = 0  # Numeric index fallback for this output");
        toml.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#B#{id}\"");
        toml.Should().Contain("# target = 1  # Numeric index fallback for this output");
    }

    [Fact]
    public void SeedExplicitConfigFromDefaults_WritesConfigTemplateAfterSuccessfulExecution()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            string configPath = Path.Combine(tempRoot, "config", "user.toml");
            List<string> warnings = [];

            GlobalOptions options = new();
            HardwareProfile hardware = new(
            [
                new OutputRecord { Id = "\\\\?\\DISPLAY#SAM#{id}", Index = 0 },
            ]);

            Program.SeedExplicitConfigFromDefaults(configPath, configExistedAtStartup: false, isDryRun: false, options, hardware, warnings);

            string seeded = File.ReadAllText(configPath);
            seeded.Should().Contain("bgraster-config.schema.json");
            seeded.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#SAM#{id}\"");
            seeded.Should().Contain("# target = 0  # Numeric index fallback for this output");
            warnings.Should().ContainSingle();
            warnings[0].Should().Contain("config template");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void BuildConfigurationErrorMessage_UsesStandardizedFormat()
    {
        string message = Program.BuildConfigurationErrorMessage(
            @"C:\temp\config.toml",
            new FormatException("Failed to parse TOML config"));

        message.Should().StartWith("bg-raster: configuration error");
        message.Should().Contain(@"C:\temp\config.toml");
        message.Should().Contain("Failed to parse TOML config");
    }

    static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}