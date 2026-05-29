namespace GameshowPro.BgRaster;

using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using GameshowPro.BgRaster.Configuration;
using GameshowPro.BgRaster.Discovery;
using GameshowPro.BgRaster.FileLifecycle;
using GameshowPro.BgRaster.Hashing;
using GameshowPro.BgRaster.Models;
using GameshowPro.BgRaster.Rendering;
using GameshowPro.BgRaster.Resolution;
using GameshowPro.BgRaster.StateCache;
using GameshowPro.BgRaster.Wallpaper;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        RootCommand root = CliBinding.BuildRootCommand(RunAsync);
        return await root.Parse(args).InvokeAsync();
    }

    static async Task<int> RunAsync(string? configPath, CliOverlay cliOverlay)
    {
        Stopwatch executionTimer = Stopwatch.StartNew();

        List<string> configurationWarnings = [];

        ImmutableArray<string> defaultConfigSearchPaths = GetDefaultConfigSearchPaths();
        string resolvedConfigPath = ResolveConfigPath(configPath, defaultConfigSearchPaths);
        bool configExists = File.Exists(resolvedConfigPath);

        GlobalOptions options;
        try
        {
            options = configExists
                ? ConfigLoader.Load(resolvedConfigPath, configurationWarnings)
                : new GlobalOptions();

            options = ConfigLoader.ApplyCliOverlay(options, cliOverlay, configurationWarnings);
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine(BuildConfigurationErrorMessage(resolvedConfigPath, ex));
            return 2;
        }

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(options.Render.MinimumLogLevel);
        });
        ILogger logger = loggerFactory.CreateLogger("bg-raster");

        int ReturnWithTiming(int exitCode)
        {
            executionTimer.Stop();
            logger.ExecutionTime(executionTimer.ElapsedMilliseconds, executionTimer.Elapsed.ToString("c", System.Globalization.CultureInfo.InvariantCulture), exitCode);
            return exitCode;
        }

        logger.RunStart(
            resolvedConfigPath,
            configExists,
            options.Outputs.Length,
            options.Render.DryRun,
            options.Render.ContinueAfterUnchanged,
            options.Render.MinimumLogLevel);

        if (options.Outputs.Length == 0)
            logger.NoConfiguredOutputs();

        foreach (string warning in configurationWarnings)
            logger.ConfigurationWarning(warning);

        logger.EffectiveConfigBegin();
        foreach (string line in EnumerateLines(LastRunWriter.BuildEffectiveConfigToml(options)))
            logger.EffectiveConfigLine(line);
        logger.EffectiveConfigEnd();

        string settingsHash = SettingsHasher.Compute(options);
        logger.SettingsHashComputed(settingsHash);

        IDisplayDiscovery discovery = new DisplayDiscovery();
        HardwareProfile hardware = discovery.Discover();
        logger.HardwareDiscovered(hardware.Outputs.Length);
        foreach (OutputRecord output in hardware.Outputs)
        {
            logger.HardwareOutput(
                output.Id,
                output.Index,
                output.DesktopX,
                output.DesktopY,
                output.WidthPx,
                output.HeightPx,
                output.Rotation,
                output.DpiX,
                output.DpiY);
        }

        bool isDryRun = options.Render.DryRun;
        bool continueAfterUnchanged = options.Render.ContinueAfterUnchanged;
        string outputDir = FileNamer.GetOutputDirectory(options.Render.Output);
        string lastRunFileName = isDryRun ? "lastRun.dry.toml" : "lastRun.toml";
        string lastRunPath = Path.Combine(outputDir, lastRunFileName);
        logger.IoPaths(outputDir, lastRunPath);

        LastRunState? lastRun = LastRunReader.Read(lastRunPath);
        logger.LastRunLoad(lastRun is not null, lastRunPath);

        if (!isDryRun && lastRun is not null)
        {
            string currentVersion = GetAssemblyVersion();
            bool unchanged = lastRun.Meta.Version == currentVersion
                && lastRun.Meta.SettingsHash == settingsHash
                && HardwareProfileMatches(lastRun.HardwareOutputs, hardware);

            logger.UnchangedCheck(unchanged, continueAfterUnchanged);
            if (unchanged)
            {
                logger.RunSkippedUnchanged();
                if (!continueAfterUnchanged)
                {
                    logger.SkipBecauseUnchanged();
                    return ReturnWithTiming(0);
                }

                logger.ContinueAfterUnchanged();
            }
        }

        logger.MatchingStart(options.Outputs.Length);
        ImmutableArray<MatchResult> matches = TargetMatcher.Match(
            hardware,
            options.Outputs,
            skipUnspecifiedOutputs: options.Render.OutputsSkipUnspecified);

        Directory.CreateDirectory(outputDir);

        OutputRenderer renderer = new();
        Dictionary<string, string> assignedFiles = [];
        Dictionary<string, string> hardwareStatuses = [];
        ImmutableArray<ConfiguredOutputStatus>.Builder configuredStatuses =
            ImmutableArray.CreateBuilder<ConfiguredOutputStatus>();


        foreach (MatchResult match in matches)
        {
            switch (match)
            {
                case MatchResult.Matched(OutputRecord output, OutputOptions outputConfig):
                    logger.RenderStart(output.Id, DescribeTarget(outputConfig.Target));
                    RenderOutcome outcome = await renderer.RenderOutputAsync(output, outputConfig, options, outputDir);
                    assignedFiles[output.Id] = outcome.FilePath;
                    hardwareStatuses[output.Id] = "output-rendered";
                    configuredStatuses.Add(new ConfiguredOutputStatus
                    {
                        TargetDescription = DescribeTarget(outputConfig.Target),
                        Status = "output-matched",
                        Reason = $"id=\"{output.Id}\" index={output.Index} position={output.DesktopX},{output.DesktopY} resolution={output.WidthPx}x{output.HeightPx}",
                        Slices = outcome.SliceStatuses,
                    });
                    logger.OutputRendered(output.Id, outcome.FilePath);
                    break;
                case MatchResult.NotFound notFound:
                    string notFoundTarget = DescribeTarget(notFound.Config.Target);
                    configuredStatuses.Add(new ConfiguredOutputStatus
                    {
                        TargetDescription = notFoundTarget,
                        Status = "output-not-found",
                    });
                    logger.OutputNotFound(notFoundTarget);
                    break;
                case MatchResult.Duplicate duplicate:
                    string dupTarget = DescribeTarget(duplicate.Config.Target);
                    configuredStatuses.Add(new ConfiguredOutputStatus
                    {
                        TargetDescription = dupTarget,
                        Status = "duplicate-output-ignored",
                    });
                    logger.DuplicateOutputIgnored(dupTarget);
                    break;
            }
        }

                logger.MatchingFinished(matches.Length, assignedFiles.Count);
                if (assignedFiles.Count == 0)
                    logger.NoRenderedFiles();

        foreach (OutputRecord hw in hardware.Outputs)
        {
            if (!hardwareStatuses.ContainsKey(hw.Id))
                hardwareStatuses[hw.Id] = "output-discovered";
        }

        RunStatus runStatus = new()
        {
            HardwareStatuses = hardwareStatuses.ToFrozenDictionary(),
            ConfiguredOutputs = configuredStatuses.ToImmutable(),
        };

        ImmutableArray<string> unrecycled = [];

        if (!isDryRun)
        {
            if (assignedFiles.Count == 0)
                logger.AssignmentSkippedNoRenderedFiles();

            IWallpaperAssigner assigner = new WallpaperAssigner();
            logger.AssignStart(assignedFiles.Count);
            bool success = await assigner.AssignAsync(assignedFiles.ToFrozenDictionary());
            logger.AssignResult(success, assignedFiles.Count);

            if (!success)
            {
                await assigner.ClearAsync(assignedFiles.Keys);
                logger.WallpaperAssignmentFailed();
                WriteLastRun(lastRunPath, settingsHash, hardware, options, assignedFiles, unrecycled, runStatus, logger);
                return ReturnWithTiming(1);
            }

            StaleFileCleaner cleaner = new();
            ImmutableArray<string> stale = cleaner.FindStaleFiles(outputDir, assignedFiles.Values.ToHashSet());
            unrecycled = cleaner.RecycleFiles(stale);
            logger.StaleScan(stale.Length, unrecycled.Length);
        }

        logger.LastRunWrite(lastRunPath, assignedFiles.Count, unrecycled.Length);
        WriteLastRun(lastRunPath, settingsHash, hardware, options, assignedFiles, unrecycled, runStatus, logger);
        SeedExplicitConfigFromDefaults(configPath, configExists, isDryRun, options, hardware, configurationWarnings);
        logger.RunComplete();
        return ReturnWithTiming(0);
    }

    static void WriteLastRun(
        string path, string settingsHash, HardwareProfile hardware,
        GlobalOptions options, Dictionary<string, string> assignedFiles,
        ImmutableArray<string> unrecycled, RunStatus runStatus, ILogger logger)
    {
        string version = GetAssemblyVersion();
        LastRunState state = new()
        {
            Meta = new LastRunMeta
            {
                Version = version,
                SettingsHash = settingsHash,
                Timestamp = DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                AssignedFiles = assignedFiles.ToFrozenDictionary(),
                UnrecycledFiles = [.. unrecycled],
            },
            HardwareOutputs = hardware.Outputs,
            EffectiveConfig = options,
        };
        LastRunWriter.Write(path, state, version, runStatus, logger);
    }

    internal static ImmutableArray<string> GetDefaultConfigSearchPaths() =>
        GetDefaultConfigSearchPaths(
            AppContext.BaseDirectory,
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    internal static ImmutableArray<string> GetDefaultConfigSearchPaths(
        string executableDirectory,
        string commonApplicationDataDirectory,
        string localApplicationDataDirectory,
        string applicationDataDirectory)
    {
        ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(4);
        builder.Add(Path.Combine(executableDirectory, "config.toml"));
        AddSearchPath(builder, commonApplicationDataDirectory);
        AddSearchPath(builder, localApplicationDataDirectory);
        AddSearchPath(builder, applicationDataDirectory);
        return builder.ToImmutable();

        static void AddSearchPath(ImmutableArray<string>.Builder builder, string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                return;

            builder.Add(Path.Combine(rootDirectory, "BgInfo", "config.toml"));
        }
    }

    internal static string ResolveConfigPath(string? explicitConfigPath, ImmutableArray<string> defaultConfigSearchPaths)
    {
        if (!string.IsNullOrWhiteSpace(explicitConfigPath))
            return explicitConfigPath;

        foreach (string candidate in defaultConfigSearchPaths)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return defaultConfigSearchPaths[0];
    }

    internal static void SeedExplicitConfigFromDefaults(
        string? explicitConfigPath,
        bool configExistedAtStartup,
        bool isDryRun,
        GlobalOptions effectiveOptions,
        HardwareProfile hardware,
        List<string>? warnings = null)
    {
        if (!ShouldSeedExplicitConfigFromDefaults(explicitConfigPath, configExistedAtStartup, isDryRun))
            return;

        string destinationConfigPath = explicitConfigPath!;

        string? directory = Path.GetDirectoryName(destinationConfigPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        string seededToml = BuildSeedConfigToml(effectiveOptions, hardware.Outputs);
        File.WriteAllText(destinationConfigPath, seededToml, Encoding.UTF8);
        warnings?.Add($"Config file '{destinationConfigPath}' did not exist at startup; wrote a config template seeded from effective defaults and detected outputs after successful execution.");
    }

    internal static bool ShouldSeedExplicitConfigFromDefaults(string? explicitConfigPath, bool configExistedAtStartup, bool isDryRun) =>
        !string.IsNullOrWhiteSpace(explicitConfigPath)
        && !configExistedAtStartup
        && !isDryRun;

    internal static string BuildSeedConfigToml(GlobalOptions effectiveOptions, ImmutableArray<OutputRecord> detectedOutputs)
    {
        GlobalOptions optionsWithoutPerOutput = effectiveOptions with { Outputs = [] };

        StringBuilder sb = new();
        sb.AppendLine("# $schema: https://raw.githubusercontent.com/gameshowpro/BgRaster/refs/heads/main/docs/schemas/bgraster-config.schema.json");
        sb.AppendLine();
        sb.AppendLine(LastRunWriter.BuildEffectiveConfigToml(optionsWithoutPerOutput));

        foreach (OutputRecord output in detectedOutputs.OrderBy(o => o.Index))
        {
            sb.AppendLine();
            sb.AppendLine("[[output]]");
            sb.AppendLine($"target = \"{EscapeTomlString(output.Id)}\"");
            sb.AppendLine($"# target = {output.Index}  # Numeric index fallback for this output");
        }

        return sb.ToString().TrimEnd();
    }

    static string EscapeTomlString(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    internal static string BuildConfigurationErrorMessage(string configPath, Exception exception) =>
        $"bg-raster: configuration error in '{configPath}': {exception.Message}";

    static bool HardwareProfileMatches(ImmutableArray<OutputRecord> stored, HardwareProfile current)
    {
        if (stored.Length != current.Outputs.Length) return false;

        ImmutableArray<OutputRecord> sortedStored = [.. stored.OrderBy(o => o.Id, StringComparer.Ordinal)];
        ImmutableArray<OutputRecord> sortedCurrent = [.. current.Outputs.OrderBy(o => o.Id, StringComparer.Ordinal)];

        for (int i = 0; i < sortedStored.Length; i++)
        {
            OutputRecord s = sortedStored[i];
            OutputRecord c = sortedCurrent[i];
            if (s.Id != c.Id || s.WidthPx != c.WidthPx || s.HeightPx != c.HeightPx
                || s.DesktopX != c.DesktopX || s.DesktopY != c.DesktopY
                || s.Rotation != c.Rotation || s.DpiX != c.DpiX || s.DpiY != c.DpiY)
                return false;
        }
        return true;
    }

    static string GetAssemblyVersion() =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";

    static string DescribeTarget(OutputTarget target) => target switch
    {
        OutputTarget.IndexTarget(int idx) => idx.ToString(System.Globalization.CultureInfo.InvariantCulture),
        OutputTarget.IdTarget(string id) => $"\"{id}\"",
        _ => "unknown",
    };



    static IEnumerable<string> EnumerateLines(string text)
    {
        using StringReader reader = new(text);
        while (reader.ReadLine() is string line)
            yield return line;
    }
}
