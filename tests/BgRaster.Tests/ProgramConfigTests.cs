// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

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
            @"C:\ProgramData\BgRaster\config.toml",
            @"C:\Users\User\AppData\Local\BgRaster\config.toml",
            @"C:\Users\User\AppData\Roaming\BgRaster\config.toml");
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
    public void ResolveConfigPath_ExpandsAndNormalizesExplicitPath()
    {
        string tempRoot = CreateTempDirectory();
        string environmentVariableName = "BGRASTER_TEST_CONFIG_ROOT";
        string? originalEnvironmentVariable = Environment.GetEnvironmentVariable(environmentVariableName);

        try
        {
            Environment.SetEnvironmentVariable(environmentVariableName, tempRoot);

            string explicitPath = $"%{environmentVariableName}%\\BgRaster\\config.toml";
            ImmutableArray<string> defaultPaths = [Path.Combine(tempRoot, "fallback.toml")];

            string resolvedPath = Program.ResolveConfigPath(explicitPath, defaultPaths);

            resolvedPath.Should().Be(Path.GetFullPath(Path.Combine(tempRoot, "BgRaster", "config.toml")));
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariableName, originalEnvironmentVariable);
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
        public void ResolveConfigPath_AppendsDefaultFilename_WhenExplicitPathIsDirectory()
        {
            string tempRoot = CreateTempDirectory();
            try
            {
                ImmutableArray<string> defaultPaths = [Path.Combine(tempRoot, "fallback.toml")];
                string resolvedPath = Program.ResolveConfigPath(tempRoot, defaultPaths);

                resolvedPath.Should().Be(Path.Combine(tempRoot, "config.toml"));
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
    public void ShouldWriteExplicitConfigOnUnchangedSkip_ReturnsTrueOnlyWhenMissingExplicitAndSkipping()
    {
        Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: false).Should().BeTrue();
        Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: true, isDryRun: false, continueAfterUnchanged: false).Should().BeFalse();
        Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: true, continueAfterUnchanged: false).Should().BeFalse();
        Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: true).Should().BeFalse();
        Program.ShouldWriteExplicitConfigOnUnchangedSkip(null, configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: false).Should().BeFalse();
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
        toml.Should().Contain("[labeled-edges]");
            toml.Should().Contain("tail-length = [\"10px\"]");
            toml.Should().Contain("thickness = [\"3px\"]");
            toml.Should().Contain("scope = [\"Desktop\"]");
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
            seeded.Should().Contain("[labeled-edges]");
                seeded.Should().Contain("tail-length = [\"10px\"]");
                seeded.Should().Contain("thickness = [\"3px\"]");
                seeded.Should().Contain("scope = [\"Desktop\"]");
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

    [Fact]
    public void IsSkiaNativeDependencyFailure_ReturnsTrue_WhenDllNotFoundIsInExceptionChain()
    {
        TypeInitializationException exception = new(
            "Skia type initializer failed",
            new DllNotFoundException("Unable to load DLL 'libSkiaSharp' or one of its dependencies."));

        bool result = Program.IsSkiaNativeDependencyFailure(exception);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsSkiaNativeDependencyFailure_ReturnsFalse_ForUnrelatedExceptions()
    {
        InvalidOperationException exception = new("Something else failed.", new DllNotFoundException("Unable to load DLL 'sqlite3'."));

        bool result = Program.IsSkiaNativeDependencyFailure(exception);

        result.Should().BeFalse();
    }

    [Fact]
    public void BuildNativeDependencyErrorMessage_ContainsActionableGuidance()
    {
        DllNotFoundException exception = new("Unable to load DLL 'libSkiaSharp' or one of its dependencies.");

        string message = Program.BuildNativeDependencyErrorMessage(exception);

        message.Should().StartWith("bg-raster: required native library 'libSkiaSharp.dll' could not be loaded.");
        message.Should().Contain("BgRaster.exe and libSkiaSharp.dll are in the same folder");
    }

    static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}