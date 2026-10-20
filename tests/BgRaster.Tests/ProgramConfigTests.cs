// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC


using GameshowPro.BgRaster;

namespace GameshowPro.BgRaster.Tests;

public class ProgramConfigTests
{
    [Fact]
    public void GetDefaultConfigSearchPaths_ReturnsRequestedHierarchy()
    {
        ImmutableArray<ConfigSearchCandidate> candidates = Program.GetDefaultConfigSearchPaths(
            executableDirectory: @"C:\Apps\BgRaster",
            commonApplicationDataDirectory: @"C:\ProgramData",
            localApplicationDataDirectory: @"C:\Users\User\AppData\Local",
            applicationDataDirectory: @"C:\Users\User\AppData\Roaming");

        _ = candidates.Should().Equal(
            new ConfigSearchCandidate(@"C:\Apps\BgRaster\config.toml", RequiresAdministratorControl: false),
            new ConfigSearchCandidate(@"C:\Users\User\AppData\Local\BgRaster\config.toml", RequiresAdministratorControl: false),
            new ConfigSearchCandidate(@"C:\Users\User\AppData\Roaming\BgRaster\config.toml", RequiresAdministratorControl: false),
            new ConfigSearchCandidate(@"C:\ProgramData\BgRaster\config.toml", RequiresAdministratorControl: true));
    }

    [Fact]
    public void ResolveConfigPath_UsesFirstExistingDefaultCandidate()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            WriteConfig(candidates[2].Path);
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(null, candidates, ThrowIfTrustChecked, warnings);

            _ = resolvedPath.Should().Be(candidates[2].Path);
            _ = warnings.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ResolveConfigPath_PrefersUserConfigOverMachineWideConfig()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            WriteConfig(candidates[1].Path);
            WriteConfig(candidates[3].Path);
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(null, candidates, ThrowIfTrustChecked, warnings);

            _ = resolvedPath.Should().Be(candidates[1].Path);
            _ = warnings.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ResolveConfigPath_UsesMachineWideConfig_WhenAdministratorsControlIt()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            WriteConfig(candidates[3].Path);
            List<string> checkedPaths = [];
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(
                null,
                candidates,
                path =>
                {
                    checkedPaths.Add(path);
                    return true;
                },
                warnings);

            _ = resolvedPath.Should().Be(candidates[3].Path);
            _ = checkedPaths.Should().Equal(candidates[3].Path);
            _ = warnings.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ResolveConfigPath_SkipsMachineWideConfigAndWarns_WhenAdministratorsDoNotControlIt()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            WriteConfig(candidates[3].Path);
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(null, candidates, _ => false, warnings);

            // Falls through to built-in defaults: the fallback path next to the executable does not exist.
            _ = resolvedPath.Should().Be(candidates[0].Path);
            _ = File.Exists(resolvedPath).Should().BeFalse();
            _ = warnings.Should().Equal(
                $"Ignored '{candidates[3].Path}': it is not controlled by administrators, so another account on this computer could have written it. See https://bgraster.gameshow.pro/developer/config-file-logic/#machine-wide-config");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ResolveConfigPath_DoesNotCheckTrustOrWarn_WhenMachineWideConfigIsMissing()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(null, candidates, ThrowIfTrustChecked, warnings);

            _ = resolvedPath.Should().Be(candidates[0].Path);
            _ = warnings.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ResolveConfigPath_DoesNotCheckTrust_ForExplicitPath()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            ImmutableArray<ConfigSearchCandidate> candidates = CreateCandidates(tempRoot);
            WriteConfig(candidates[3].Path);
            List<string> warnings = [];

            string resolvedPath = Program.ResolveConfigPath(candidates[3].Path, candidates, ThrowIfTrustChecked, warnings);

            _ = resolvedPath.Should().Be(candidates[3].Path);
            _ = warnings.Should().BeEmpty();
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
            ImmutableArray<ConfigSearchCandidate> defaultPaths = [new(Path.Combine(tempRoot, "fallback.toml"), RequiresAdministratorControl: false)];

            string resolvedPath = Program.ResolveConfigPath(explicitPath, defaultPaths, ThrowIfTrustChecked, []);

            _ = resolvedPath.Should().Be(Path.GetFullPath(Path.Combine(tempRoot, "BgRaster", "config.toml")));
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
            ImmutableArray<ConfigSearchCandidate> defaultPaths = [new(Path.Combine(tempRoot, "fallback.toml"), RequiresAdministratorControl: false)];
            string resolvedPath = Program.ResolveConfigPath(tempRoot, defaultPaths, ThrowIfTrustChecked, []);

            _ = resolvedPath.Should().Be(Path.Combine(tempRoot, "config.toml"));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void ShouldSeedExplicitConfigFromDefaults_ReturnsTrueOnlyForMissingExplicitNonDryRunConfig()
    {
        _ = Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false).Should().BeTrue();
        _ = Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: true, isDryRun: false).Should().BeFalse();
        _ = Program.ShouldSeedExplicitConfigFromDefaults(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: true).Should().BeFalse();
        _ = Program.ShouldSeedExplicitConfigFromDefaults(null, configExistedAtStartup: false, isDryRun: false).Should().BeFalse();
    }

    [Fact]
    public void GetSeedConfigPath_NeverSeedsTheFallbackPathNextToTheExecutable()
    {
        string fallback = Program.ResolveConfigPath(null, Program.GetDefaultConfigSearchPaths(@"C:\app", @"C:\pd", @"C:\lad", @"C:\ad"), _ => true, []);

        _ = Program.GetSeedConfigPath(null, fallback).Should().BeNull();
        _ = Program.GetSeedConfigPath("  ", fallback).Should().BeNull();
        _ = Program.GetSeedConfigPath("cfg.toml", @"C:\work\cfg.toml").Should().Be(@"C:\work\cfg.toml");
    }

    [Fact]
    public void ShouldWriteExplicitConfigOnUnchangedSkip_ReturnsTrueOnlyWhenMissingExplicitAndSkipping()
    {
        _ = Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: false).Should().BeTrue();
        _ = Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: true, isDryRun: false, continueAfterUnchanged: false).Should().BeFalse();
        _ = Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: true, continueAfterUnchanged: false).Should().BeFalse();
        _ = Program.ShouldWriteExplicitConfigOnUnchangedSkip(@"C:\cfg.toml", configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: true).Should().BeFalse();
        _ = Program.ShouldWriteExplicitConfigOnUnchangedSkip(null, configExistedAtStartup: false, isDryRun: false, continueAfterUnchanged: false).Should().BeFalse();
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

        _ = toml.Should().Contain("bgraster-config.schema.json");
        _ = toml.Should().Contain("[labeled-edges]");
        _ = toml.Should().Contain("tail-length = [\"10px\"]");
        _ = toml.Should().Contain("thickness = [\"3px\"]");
        _ = toml.Should().Contain("scope = [\"Desktop\"]");
        _ = toml.Should().Contain("[[output]]");
        _ = toml.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#A#{id}\"");
        _ = toml.Should().Contain("# target = 0  # Numeric index fallback for this output");
        _ = toml.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#B#{id}\"");
        _ = toml.Should().Contain("# target = 1  # Numeric index fallback for this output");
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
            _ = seeded.Should().Contain("bgraster-config.schema.json");
            _ = seeded.Should().Contain("[labeled-edges]");
            _ = seeded.Should().Contain("tail-length = [\"10px\"]");
            _ = seeded.Should().Contain("thickness = [\"3px\"]");
            _ = seeded.Should().Contain("scope = [\"Desktop\"]");
            _ = seeded.Should().Contain("target = \"\\\\\\\\?\\\\DISPLAY#SAM#{id}\"");
            _ = seeded.Should().Contain("# target = 0  # Numeric index fallback for this output");
            _ = warnings.Should().ContainSingle();
            _ = warnings[0].Should().Contain("config template");
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

        _ = message.Should().StartWith("bg-raster: configuration error");
        _ = message.Should().Contain(@"C:\temp\config.toml");
        _ = message.Should().Contain("Failed to parse TOML config");
    }

    [Fact]
    public void IsSkiaNativeDependencyFailure_ReturnsTrue_WhenDllNotFoundIsInExceptionChain()
    {
        TypeInitializationException exception = new(
            "Skia type initializer failed",
            new DllNotFoundException("Unable to load DLL 'libSkiaSharp' or one of its dependencies."));

        bool result = Program.IsSkiaNativeDependencyFailure(exception);

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void IsSkiaNativeDependencyFailure_ReturnsFalse_ForUnrelatedExceptions()
    {
        InvalidOperationException exception = new("Something else failed.", new DllNotFoundException("Unable to load DLL 'sqlite3'."));

        bool result = Program.IsSkiaNativeDependencyFailure(exception);

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void BuildNativeDependencyErrorMessage_ContainsActionableGuidance()
    {
        DllNotFoundException exception = new("Unable to load DLL 'libSkiaSharp' or one of its dependencies.");

        string message = Program.BuildNativeDependencyErrorMessage();

        _ = message.Should().StartWith("bg-raster: required native library 'libSkiaSharp.dll' could not be loaded.");
        _ = message.Should().Contain("BgRaster.exe and libSkiaSharp.dll are in the same folder");
    }

    private static ImmutableArray<ConfigSearchCandidate> CreateCandidates(string tempRoot) =>
        Program.GetDefaultConfigSearchPaths(
            Path.Combine(tempRoot, "exe"),
            Path.Combine(tempRoot, "programData"),
            Path.Combine(tempRoot, "localAppData"),
            Path.Combine(tempRoot, "appData"));

    private static void WriteConfig(string path)
    {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "[render]\nverbosity = \"normal\"\n");
    }

    private static bool ThrowIfTrustChecked(string path) =>
        throw new InvalidOperationException($"The trust check must not run for '{path}'.");

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-tests-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(path);
        return path;
    }
}