// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster;

using System.Diagnostics;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Any(a => a == "--version"))
            {
                Console.WriteLine(CliBinding.GetVersionString());
                return 0;
            }

            RootCommand root = CliBinding.BuildRootCommand(RunAsync);
            return await root.Parse(args).InvokeAsync();
        }
        catch (Exception ex) when (IsSkiaNativeDependencyFailure(ex))
        {
            Console.Error.WriteLine(BuildNativeDependencyErrorMessage(ex));
            return 3;
        }
    }

    static async Task<int> RunAsync(string? configPath, CliOverlay cliOverlay)
    {
        Stopwatch executionTimer = Stopwatch.StartNew();

        List<string> configurationWarnings = [];

        ImmutableArray<string> defaultConfigSearchPaths = GetDefaultConfigSearchPaths();
        string resolvedConfigPath = ResolveConfigPath(configPath, defaultConfigSearchPaths);
        bool configExists = File.Exists(resolvedConfigPath);

        if (!string.IsNullOrWhiteSpace(configPath) && !configExists)
        {
            Console.Error.WriteLine($"bg-raster: config file not found: '{resolvedConfigPath}'");
            return 1;
        }

        GlobalOptions options;
        GlobalOptions optionsFromToml;
        GlobalOptions optionsDefaults;
        HashSet<string> tomlPaths;
        HashSet<string> cliPaths;
        try
        {
            if (configExists)
            {
                options = ConfigLoader.Load(resolvedConfigPath, out tomlPaths, configurationWarnings);
            }
            else
            {
                options = new GlobalOptions();
                tomlPaths = new HashSet<string>(StringComparer.Ordinal);
            }

            optionsFromToml = options;  // snapshot after TOML parse, before CLI overlay
            optionsDefaults = new GlobalOptions();  // pure schema defaults

            options = ConfigLoader.ApplyCliOverlay(options, cliOverlay, out cliPaths, configurationWarnings);

            if (options.Render.NoDiscovery && !options.Render.DryRun)
            {
                configurationWarnings.Add("[render].no-discovery implies [render].no-assignment=true; wallpaper assignment will be skipped.");
                options = options with { Render = options.Render with { DryRun = true } };
            }
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine(BuildConfigurationErrorMessage(resolvedConfigPath, ex));
            return 2;
        }

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
#pragma warning disable IL2026, IL3050 // AOT: generic formatter overload
            builder.AddConsoleFormatter<BgRasterConsoleFormatter, ConsoleFormatterOptions>();
#pragma warning restore IL2026, IL3050
            builder.AddConsole(o => o.FormatterName = "BgRaster");
            builder.SetMinimumLevel(options.Render.MinimumLogLevel);
        });
        ILogger logger = loggerFactory.CreateLogger("bg-raster");

        int ReturnWithTiming(int exitCode)
                {
                    executionTimer.Stop();
                    if (logger.IsEnabled(LogLevel.Information))
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

        if (logger.IsEnabled(LogLevel.Debug))
        {
            string effectiveToml = LastRunWriter.BuildEffectiveConfigToml(options);
            string colorized = TomlColorizer.ColorizeProvenance(effectiveToml, tomlPaths, cliPaths);
            logger.BlockBeginEffectiveConfig();
            Console.WriteLine(colorized);
            logger.BlockEndEffectiveConfig();
        }

        string settingsHash = SettingsHasher.Compute(options);
        logger.SettingsHashComputed(settingsHash);

        bool noDiscovery = options.Render.NoDiscovery;
        ImmutableArray<(OutputRecord Output, OutputOptions Config)> noDiscoveryMappings = [];

        HardwareProfile hardware;
        if (noDiscovery)
        {
            noDiscoveryMappings = BuildNoDiscoveryMappings(options.Outputs, configurationWarnings);
            hardware = new HardwareProfile([.. noDiscoveryMappings.Select(mapping => mapping.Output)]);
        }
        else
        {
            IDisplayDiscovery discovery = new DisplayDiscovery();
            hardware = discovery.Discover();
        }

        foreach (OutputOptions output in options.Outputs)
        {
            if (!noDiscovery && output.HardwareOutput is not null)
                logger.ConfigurationWarning("[output.hardware_output] is ignored unless [render].no-discovery=true.");
        }

        (int systemWidthPx, int systemHeightPx) = GetSystemDimensions(hardware);
        if (!noDiscovery && logger.IsEnabled(LogLevel.Debug))
        {
            const string D = "\x1b[2m";
            const string G = "\x1b[32m";
            const string C = "\x1b[36m";
            const string R = "\x1b[0m";
            Console.WriteLine();
            logger.BlockBeginDisplayInfo();
            Console.WriteLine($"{D}count{R}={C}{hardware.Outputs.Length}{R}");
            foreach (OutputRecord output in hardware.Outputs)
            {
                Console.WriteLine($"  {D}id{R}={G}\"{EscapeTomlString(output.Id)}\"{R}");
                Console.WriteLine($"  {D}index{R}={C}{output.Index}{R}");
                Console.WriteLine($"  {D}position{R}={C}{output.DesktopX},{output.DesktopY}{R}");
                Console.WriteLine($"  {D}resolution{R}={C}{output.WidthPx}x{output.HeightPx}{R}");
                Console.WriteLine($"  {D}rotation{R}={C}{output.Rotation}{R}");
                Console.WriteLine($"  {D}dpi{R}={C}{output.DpiX}x{output.DpiY}{R}");
                Console.WriteLine($"  {D}adapter{R}={G}\"{EscapeTomlString(output.AdapterName)}\"{R}");
                Console.WriteLine($"  {D}friendlyName{R}={G}\"{EscapeTomlString(output.FriendlyName)}\"{R}");
                Console.WriteLine("  ---");
            }
            logger.BlockEndDisplayInfo();
        }

        if (!noDiscovery && logger.IsEnabled(LogLevel.Debug))
        {
            const string D = "\x1b[2m";
            const string G = "\x1b[32m";
            const string C = "\x1b[36m";
            const string R = "\x1b[0m";

            ImmutableArray<AdapterInfo> networkAdapters = options.Render.SimulateNetwork
                ? NetworkSimulator.GetAdapters()
                : NetworkCollector.Collect();

            Console.WriteLine();
            logger.BlockBeginNetworkInfo();
            Console.WriteLine($"{D}count{R}={C}{networkAdapters.Length}{R}");
            foreach (AdapterInfo adapter in networkAdapters)
            {
                Console.WriteLine($"  {D}name{R}={G}\"{EscapeTomlString(adapter.Name)}\"{R}");
                Console.WriteLine($"  {D}description{R}={G}\"{EscapeTomlString(adapter.Description)}\"{R}");
                Console.WriteLine($"  {D}id{R}={G}\"{EscapeTomlString(adapter.Id)}\"{R}");
                string typeLine = $"  {D}type{R}={G}\"{EscapeTomlString(adapter.Type)}\"{R}";
                if (!string.Equals(adapter.Type, adapter.TypeLong, StringComparison.Ordinal))
                    typeLine += $"{D} ({EscapeTomlString(adapter.TypeLong)}){R}";
                Console.WriteLine(typeLine);
                string statusColor = adapter.Status == "Up" ? G : R;
                Console.WriteLine($"  {D}status{R}={statusColor}\"{EscapeTomlString(adapter.Status)}\"{R}");
                Console.WriteLine($"  {D}speed{R}={G}\"{EscapeTomlString(adapter.Speed)}\"{R}");
                Console.WriteLine($"  {D}mac{R}={G}\"{EscapeTomlString(adapter.MacAddress)}\"{R}");
                Console.WriteLine($"  {D}ips{R}={C}{adapter.IpAddresses.Length}{R}");
                foreach (AdapterIpAddress ip in adapter.IpAddresses)
                {
                    string cidr = ip.CidrBits > 0 ? $"/{ip.CidrBits}" : "";
                    Console.WriteLine($"    {C}{ip.Address}{cidr}{R} {D}origin{R}={ip.Origin}");
                }
                Console.WriteLine("  ---");
            }
            logger.BlockEndNetworkInfo();
        }

        bool isDryRun = options.Render.DryRun;
        bool continueAfterUnchanged = options.Render.ContinueAfterUnchanged;
        string outputTemplate = FileNamer.GetOutputTemplate(options.Render.Output);
        string outputDir = FileNamer.GetOutputDirectory(outputTemplate);
        string lastRunFileName = isDryRun ? "lastRun.dry.toml" : "lastRun.toml";
        string lastRunPath = Path.Combine(outputDir, lastRunFileName);
        logger.IoPaths(outputDir, lastRunPath);

        if (hardware.Outputs.Length > 1
            && !FileNamer.ContainsToken(outputTemplate, "index")
            && !FileNamer.ContainsToken(outputTemplate, "friendlyName"))
        {
            logger.ConfigurationWarning("render-output does not include {index} or {friendlyName}; multiple outputs can overwrite each other.");
        }

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
        ImmutableArray<MatchResult> matches = noDiscovery
            ? []
            : TargetMatcher.Match(
                hardware,
                options.Outputs,
                skipUnspecifiedOutputs: options.Render.OutputsSkipUnspecified);

        Directory.CreateDirectory(outputDir);

        OutputRenderer renderer = new();
        Dictionary<string, string> assignedFiles = [];
        Dictionary<string, string> hardwareStatuses = [];
        ImmutableArray<ConfiguredOutputStatus>.Builder configuredStatuses =
            ImmutableArray.CreateBuilder<ConfiguredOutputStatus>();

        if (noDiscovery)
        {
            foreach ((OutputRecord Output, OutputOptions Config) in noDiscoveryMappings)
            {
                string targetDescription = DescribeTarget(Config.Target);
                logger.RenderStart(Output.Id, targetDescription);
                FileNamer.RenderOutputPathResult outputPath = FileNamer.ResolveRenderOutputPath(outputTemplate, Output, options.Render.MachineName);
                foreach (string warning in outputPath.Warnings)
                    logger.ConfigurationWarning(warning);

                RenderOutcome outcome = await renderer.RenderOutputAsync(Output, Config, options, outputPath.FilePath, systemWidthPx, systemHeightPx);
                assignedFiles[Output.Id] = outcome.FilePath;
                hardwareStatuses[Output.Id] = "output-rendered";
                configuredStatuses.Add(new ConfiguredOutputStatus
                {
                    TargetDescription = targetDescription,
                    Status = "output-matched",
                    Reason = $"id=\"{Output.Id}\" index={Output.Index} position={Output.DesktopX},{Output.DesktopY} resolution={Output.WidthPx}x{Output.HeightPx}",
                    Slices = outcome.SliceStatuses,
                });
                logger.OutputRendered(Output.Id, outcome.FilePath);
            }
        }
        else
        {
            foreach (MatchResult match in matches)
            {
                switch (match)
                {
                    case MatchResult.Matched(OutputRecord output, OutputOptions outputConfig):
                                            if (logger.IsEnabled(LogLevel.Trace))
                                                logger.RenderStart(output.Id, DescribeTarget(outputConfig.Target));
                        FileNamer.RenderOutputPathResult outputPath = FileNamer.ResolveRenderOutputPath(outputTemplate, output, options.Render.MachineName);
                        foreach (string warning in outputPath.Warnings)
                            logger.ConfigurationWarning(warning);

                        RenderOutcome outcome = await renderer.RenderOutputAsync(output, outputConfig, options, outputPath.FilePath, systemWidthPx, systemHeightPx);
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
        }

                logger.MatchingFinished(matches.Length, assignedFiles.Count);
                if (assignedFiles.Count == 0)
                    logger.NoRenderedFiles();

        foreach (OutputRecord hw in hardware.Outputs)
        {
            if (!noDiscovery && !hardwareStatuses.ContainsKey(hw.Id))
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
            ImmutableArray<string> stale = StaleFileCleaner.FindStaleFiles(outputDir, assignedFiles.Values.ToHashSet());
            unrecycled = StaleFileCleaner.RecycleFiles(stale);
            logger.StaleScan(stale.Length, unrecycled.Length);
        }

        logger.LastRunWrite(lastRunPath, assignedFiles.Count, unrecycled.Length);
        WriteLastRun(lastRunPath, settingsHash, hardware, options, assignedFiles, unrecycled, runStatus, logger);
        SeedExplicitConfigFromDefaults(resolvedConfigPath, configExists, isDryRun, options, hardware, configurationWarnings);
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

            builder.Add(Path.Combine(rootDirectory, "BgRaster", "config.toml"));
        }
    }

    internal static string ResolveConfigPath(string? explicitConfigPath, ImmutableArray<string> defaultConfigSearchPaths)
    {
        if (!string.IsNullOrWhiteSpace(explicitConfigPath))
        {
            string resolved = ConfiguredPathResolver.Resolve(explicitConfigPath, Directory.GetCurrentDirectory());

            // If the resolved path is an existing directory, append the default filename
            if (Directory.Exists(resolved))
                return Path.Combine(resolved, "config.toml");

            return resolved;
        }

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

    // Compatibility helper retained for tests while unchanged-skip behavior stays centralized.
    internal static bool ShouldWriteExplicitConfigOnUnchangedSkip(
        string? explicitConfigPath,
        bool configExistedAtStartup,
        bool isDryRun,
        bool continueAfterUnchanged) =>
        ShouldSeedExplicitConfigFromDefaults(explicitConfigPath, configExistedAtStartup, isDryRun)
        && !continueAfterUnchanged;

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

    internal static bool IsSkiaNativeDependencyFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is DllNotFoundException dllNotFoundException
                && dllNotFoundException.Message.Contains("libSkiaSharp", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static string BuildNativeDependencyErrorMessage(Exception _)
    {
        return "bg-raster: required native library 'libSkiaSharp.dll' could not be loaded. "
            + "Ensure BgRaster.exe and libSkiaSharp.dll are in the same folder, then run again.";
    }

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

    static (int WidthPx, int HeightPx) GetSystemDimensions(HardwareProfile hardware)
    {
        if (hardware.Outputs.IsEmpty)
            return (0, 0);

        int minX = hardware.Outputs.Min(output => output.DesktopX);
        int minY = hardware.Outputs.Min(output => output.DesktopY);
        int maxX = hardware.Outputs.Max(output => output.DesktopX + output.WidthPx);
        int maxY = hardware.Outputs.Max(output => output.DesktopY + output.HeightPx);
        return (maxX - minX, maxY - minY);
    }


    static ImmutableArray<(OutputRecord Output, OutputOptions Config)> BuildNoDiscoveryMappings(
        ImmutableArray<OutputOptions> configuredOutputs,
        List<string> warnings)
    {
        ImmutableArray<(OutputRecord Output, OutputOptions Config)>.Builder builder =
            ImmutableArray.CreateBuilder<(OutputRecord Output, OutputOptions Config)>(configuredOutputs.Length);

        for (int i = 0; i < configuredOutputs.Length; i++)
        {
            OutputOptions output = configuredOutputs[i];
            OutputRecord hardware = ResolveHardwareOutput(output, i, warnings);
            builder.Add((hardware, output));
        }

        return builder.ToImmutable();
    }

    static OutputRecord ResolveHardwareOutput(OutputOptions output, int fallbackIndex, List<string> warnings)
    {
        if (output.HardwareOutput is null)
        {
            warnings.Add($"[[output]] index {fallbackIndex}: missing [output.hardware_output] while [render].no-discovery=true; using default fixed 640x480 hardware output.");
            return BuildDefaultHardwareOutput(fallbackIndex);
        }

        OutputRecord reference = output.HardwareOutput;
        int resolvedIndex = reference.Index >= 0 ? reference.Index : fallbackIndex;

        return new OutputRecord
        {
            Id = string.IsNullOrWhiteSpace(reference.Id) ? $"FIXED-{resolvedIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)}" : reference.Id,
            Index = resolvedIndex,
            DesktopX = reference.DesktopX,
            DesktopY = reference.DesktopY,
            WidthPx = reference.WidthPx > 0 ? reference.WidthPx : 640,
            HeightPx = reference.HeightPx > 0 ? reference.HeightPx : 480,
            DpiX = reference.DpiX > 0 ? reference.DpiX : 96,
            DpiY = reference.DpiY > 0 ? reference.DpiY : 96,
            Rotation = reference.Rotation,
            AdapterName = string.IsNullOrWhiteSpace(reference.AdapterName) ? "FIXED" : reference.AdapterName,
            FriendlyName = string.IsNullOrWhiteSpace(reference.FriendlyName) ? "Unspecified" : reference.FriendlyName,
        };
    }

    static OutputRecord BuildDefaultHardwareOutput(int index) => new()
    {
        Id = $"FIXED-{index.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
        Index = index,
        DesktopX = 0,
        DesktopY = 0,
        WidthPx = 640,
        HeightPx = 480,
        DpiX = 96,
        DpiY = 96,
        Rotation = 0,
        AdapterName = "FIXED",
        FriendlyName = "Unspecified",
    };
}