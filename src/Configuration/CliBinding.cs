// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

namespace GameshowPro.BgRaster.Configuration;

internal static class CliBinding
{
    private const string CBold = "\x1b[1m";
    private const string CDim = "\x1b[2m";
    private const string CReset = "\x1b[0m";
    private const string CGreen = "\x1b[32m";
    private const string CCyan = "\x1b[36m";

    internal static RootCommand BuildRootCommand(Func<string?, CliOverlay, Task<int>> handler)
    {
        Option<string?> configOption = CreateOption("--config");
        Option<string?> machineNameOption = CreateOption("--machine-name");
        Option<string?> textFormatOption = CreateOption("--text-format");
        Option<string?> textSizeOption = CreateOption("--text-size");
        Option<string?> textColorOption = CreateOption("--text-color");
        Option<string?> textXOption = CreateOption("--text-x");
        Option<string?> textYOption = CreateOption("--text-y");
        Option<string?> bgColorOption = CreateOption("--background-color");
        Option<string?> bgImageOption = CreateOption("--background-image");
        Option<string?> bgFitOption = CreateOption("--background-fit");
        Option<bool?> bgAlternatingOption = CreateNullableBoolOption("--background-alternating");
        Option<bool?> bgBorderOption = CreateNullableBoolOption("--background-border");
        Option<string?> bgBorderColorOption = CreateOption("--background-border-color");
        Option<string?> gridSizeOption = CreateOption("--grid-size");
        Option<string?> gridOddColorOption = CreateOption("--grid-odd-color");
        Option<string?> gridEvenColorOption = CreateOption("--grid-even-color");
        Option<string?> gridStrokeOption = CreateOption("--grid-stroke");
        Option<string?> gridOffsetXOption = CreateOption("--grid-offset-x");
        Option<string?> gridOffsetYOption = CreateOption("--grid-offset-y");
        Option<bool?> gridCoordinatesOption = CreateNullableBoolOption("--grid-coordinates");
        Option<string?> circleSizeOption = CreateOption("--circle-size");
        Option<string?> circleXOption = CreateOption("--circle-x");
        Option<string?> circleYOption = CreateOption("--circle-y");
        Option<string?> circleColorOption = CreateOption("--circle-color");
        Option<string?> circleStrokeOption = CreateOption("--circle-stroke");
        Option<string?> crosshairLengthOption = CreateOption("--crosshair-length");
        Option<string?> crosshairXOption = CreateOption("--crosshair-x");
        Option<string?> crosshairYOption = CreateOption("--crosshair-y");
        Option<string?> crosshairColorOption = CreateOption("--crosshair-color");
        Option<string?> crosshairStrokeOption = CreateOption("--crosshair-stroke");
        Option<string?> logoSourceOption = CreateOption("--logo-source");
        Option<string?> logoXOption = CreateOption("--logo-x");
        Option<string?> logoYOption = CreateOption("--logo-y");
        Option<string?> logoAnchorXOption = CreateOption("--logo-anchor-x");
        Option<string?> logoAnchorYOption = CreateOption("--logo-anchor-y");
        Option<string?> logoWidthOption = CreateOption("--logo-width");
        Option<string?> logoHeightOption = CreateOption("--logo-height");
        Option<string?> logoOpacityOption = CreateOption("--logo-opacity");
        Option<bool?> dryRunOption = CreateNullableBoolOption("--no-assignment");
        Option<bool?> noDiscoveryOption = CreateNullableBoolOption("--no-discovery");
        Option<bool?> outputsSkipUnspecifiedOption = CreateNullableBoolOption("--outputs-skip-unspecified");
        Option<string?> outputDirOption = CreateOption("--render-output");
        Option<bool?> continueAfterUnchangedOption = CreateNullableBoolOption("--render-force");
        Option<string?> verbosityOption = CreateOption("--verbosity");

        Option<string?> NetworkRequireAdapterTypeOption = CreateOption("--network-require-adapter-type");
        Option<bool?> networkRequireUpOption = CreateNullableBoolOption("--network-require-up");
        Option<string?> networkRequireFamilyOption = CreateOption("--network-require-family");
        Option<string?> networkAdapterFormatOption = CreateOption("--network-adapter-format");
        Option<string?> networkIpAddressFormatOption = CreateOption("--network-ip-address-format");
        Option<string?> networkXOption = CreateOption("--network-x");
        Option<string?> networkYOption = CreateOption("--network-y");
        Option<string?> networkSizeOption = CreateOption("--network-size");
        Option<string?> networkColorOption = CreateOption("--network-color");
        Option<bool?> networkRenderOption = CreateNullableBoolOption("--network-render");

        // Category mapping for help output
        Dictionary<string, List<Option>> categorized = new()
        {
            ["Frequent"] = new() { configOption, verbosityOption, continueAfterUnchangedOption, dryRunOption, outputDirOption },
            ["Advanced"] = new() { machineNameOption, noDiscoveryOption, outputsSkipUnspecifiedOption },
            ["Appearance"] = new()
        };
        Option[] appearanceOptions = {
            textFormatOption, textSizeOption, textColorOption, textXOption, textYOption,
            bgColorOption, bgImageOption, bgFitOption, bgAlternatingOption, bgBorderOption, bgBorderColorOption,
            gridSizeOption, gridOddColorOption, gridEvenColorOption, gridStrokeOption,
            gridOffsetXOption, gridOffsetYOption, gridCoordinatesOption,
            circleSizeOption, circleXOption, circleYOption, circleColorOption, circleStrokeOption,
            crosshairLengthOption, crosshairXOption, crosshairYOption, crosshairColorOption, crosshairStrokeOption,
            logoSourceOption, logoXOption, logoYOption, logoAnchorXOption, logoAnchorYOption, logoWidthOption, logoHeightOption, logoOpacityOption,
            NetworkRequireAdapterTypeOption, networkRequireUpOption, networkRequireFamilyOption, networkAdapterFormatOption, networkIpAddressFormatOption,
            networkXOption, networkYOption, networkSizeOption, networkColorOption, networkRenderOption
        };
        categorized["Appearance"].AddRange(appearanceOptions);

        RootCommand root = new($"BgRaster - per-output wallpaper renderer{CDim}  https://bgraster.gameshow.pro{CReset}");
                root.Options.Add(configOption);
                root.Options.Add(verbosityOption);
                root.Options.Add(continueAfterUnchangedOption);
                root.Options.Add(dryRunOption);
                root.Options.Add(outputDirOption);
                root.Options.Add(machineNameOption);
                root.Options.Add(noDiscoveryOption);
                root.Options.Add(outputsSkipUnspecifiedOption);
                root.Options.Add(textFormatOption);
                root.Options.Add(textSizeOption);
                root.Options.Add(textColorOption);
                root.Options.Add(textXOption);
                root.Options.Add(textYOption);
                root.Options.Add(bgColorOption);
                root.Options.Add(bgImageOption);
                root.Options.Add(bgFitOption);
                root.Options.Add(bgAlternatingOption);
                root.Options.Add(bgBorderOption);
                root.Options.Add(bgBorderColorOption);
                root.Options.Add(gridSizeOption);
                root.Options.Add(gridOddColorOption);
                root.Options.Add(gridEvenColorOption);
                root.Options.Add(gridStrokeOption);
                root.Options.Add(gridOffsetXOption);
                root.Options.Add(gridOffsetYOption);
                root.Options.Add(gridCoordinatesOption);
                root.Options.Add(circleSizeOption);
                root.Options.Add(circleXOption);
                root.Options.Add(circleYOption);
                root.Options.Add(circleColorOption);
                root.Options.Add(circleStrokeOption);
                root.Options.Add(crosshairLengthOption);
                root.Options.Add(crosshairXOption);
                root.Options.Add(crosshairYOption);
                root.Options.Add(crosshairColorOption);
                root.Options.Add(crosshairStrokeOption);
                root.Options.Add(logoSourceOption);
                root.Options.Add(logoXOption);
                root.Options.Add(logoYOption);
                root.Options.Add(logoAnchorXOption);
                root.Options.Add(logoAnchorYOption);
                root.Options.Add(logoWidthOption);
                root.Options.Add(logoHeightOption);
                root.Options.Add(logoOpacityOption);
                root.Options.Add(NetworkRequireAdapterTypeOption);
                root.Options.Add(networkRequireUpOption);
                root.Options.Add(networkRequireFamilyOption);
                root.Options.Add(networkAdapterFormatOption);
                root.Options.Add(networkIpAddressFormatOption);
                root.Options.Add(networkXOption);
                root.Options.Add(networkYOption);
                root.Options.Add(networkSizeOption);
                root.Options.Add(networkColorOption);
                root.Options.Add(networkRenderOption);

        root.SetAction(async (parseResult, ct) =>
        {
            string? config = parseResult.GetValue(configOption);
            CliOverlay overlay = new()
            {
                MachineName = parseResult.GetValue(machineNameOption),
                TextFormat = parseResult.GetValue(textFormatOption),
                TextSize = parseResult.GetValue(textSizeOption),
                TextColor = parseResult.GetValue(textColorOption),
                TextX = parseResult.GetValue(textXOption),
                TextY = parseResult.GetValue(textYOption),
                BackgroundColor = parseResult.GetValue(bgColorOption),
                BackgroundImage = parseResult.GetValue(bgImageOption),
                BackgroundFit = parseResult.GetValue(bgFitOption),
                BackgroundAlternating = parseResult.GetValue(bgAlternatingOption),
                BackgroundBorder = parseResult.GetValue(bgBorderOption),
                BackgroundBorderColor = parseResult.GetValue(bgBorderColorOption),
                GridSize = parseResult.GetValue(gridSizeOption),
                GridOddColor = parseResult.GetValue(gridOddColorOption),
                GridEvenColor = parseResult.GetValue(gridEvenColorOption),
                GridStroke = parseResult.GetValue(gridStrokeOption),
                GridOffsetX = parseResult.GetValue(gridOffsetXOption),
                GridOffsetY = parseResult.GetValue(gridOffsetYOption),
                GridCoordinates = parseResult.GetValue(gridCoordinatesOption),
                CircleSize = parseResult.GetValue(circleSizeOption),
                CircleX = parseResult.GetValue(circleXOption),
                CircleY = parseResult.GetValue(circleYOption),
                CircleColor = parseResult.GetValue(circleColorOption),
                CircleStroke = parseResult.GetValue(circleStrokeOption),
                CrosshairLength = parseResult.GetValue(crosshairLengthOption),
                CrosshairX = parseResult.GetValue(crosshairXOption),
                CrosshairY = parseResult.GetValue(crosshairYOption),
                CrosshairColor = parseResult.GetValue(crosshairColorOption),
                CrosshairStroke = parseResult.GetValue(crosshairStrokeOption),
                LogoSource = parseResult.GetValue(logoSourceOption),
                LogoX = parseResult.GetValue(logoXOption),
                LogoY = parseResult.GetValue(logoYOption),
                LogoAnchorX = parseResult.GetValue(logoAnchorXOption),
                LogoAnchorY = parseResult.GetValue(logoAnchorYOption),
                LogoWidth = parseResult.GetValue(logoWidthOption),
                LogoHeight = parseResult.GetValue(logoHeightOption),
                LogoOpacity = parseResult.GetValue(logoOpacityOption),
                RenderDryRun = parseResult.GetValue(dryRunOption),
                RenderNoDiscovery = parseResult.GetValue(noDiscoveryOption),
                RenderOutputsSkipUnspecified = parseResult.GetValue(outputsSkipUnspecifiedOption),
                RenderOutput = parseResult.GetValue(outputDirOption),
                RenderVerbosity = parseResult.GetValue(verbosityOption),
                RenderContinueAfterUnchanged = parseResult.GetValue(continueAfterUnchangedOption),
                NetworkRequireAdapterType = parseResult.GetValue(NetworkRequireAdapterTypeOption),
                NetworkRequireUp = parseResult.GetValue(networkRequireUpOption),
                NetworkRequireFamily = parseResult.GetValue(networkRequireFamilyOption),
                NetworkAdapterFormat = parseResult.GetValue(networkAdapterFormatOption),
                NetworkIpAddressFormat = parseResult.GetValue(networkIpAddressFormatOption),
                NetworkX = parseResult.GetValue(networkXOption),
                NetworkY = parseResult.GetValue(networkYOption),
                NetworkSize = parseResult.GetValue(networkSizeOption),
                NetworkColor = parseResult.GetValue(networkColorOption),
                NetworkRender = parseResult.GetValue(networkRenderOption),
            };
            return await handler(config, overlay);
        });

        return root;

        static Option<string?> CreateOption(string alias)
                {
                    CliOptionDefinition def = GetByAlias(alias);
                    string desc = ColorizeDescription(def.HelpDescription);
                    return new Option<string?>(alias) { Description = desc };
                }

                static Option<bool?> CreateNullableBoolOption(string alias)
                {
                    CliOptionDefinition def = GetByAlias(alias);
                    string desc = ColorizeDescription(def.HelpDescription);
                    return new Option<bool?>(alias) { Description = desc };
                }

                /// <summary>
                                /// Colors a description string: base text in magenta, backtick-wrapped values in green.
                                /// Backtick characters are suppressed — color serves as the visual delimiter.
                                /// Returns uncolored text when stdout is redirected.
                                /// </summary>
                                static string ColorizeDescription(string desc)
                                                                {
                                                                    if (!ConsoleSupportsAnsi())
                                                                        return desc.Replace("`", "");
                                    const string CMagenta = "\x1b[35m";
                                    string colored = System.Text.RegularExpressions.Regex.Replace(
                                        desc, @"`([^`]+)`", $"{CGreen}$1{CMagenta}");
                                    return $"{CMagenta}{colored}{CReset}";
                                }

        static CliOptionDefinition GetByAlias(string alias) =>
            GeneratedCliOptionCatalog.Definitions.First(definition =>
                string.Equals(definition.Alias, alias, StringComparison.Ordinal));
    }

    /// <summary>
        /// Writes categorized help output with section headers.
        /// Call this from Program.cs when --help is detected.
        /// </summary>

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>Returns true when the output console supports ANSI escape sequences.</summary>
        private static bool ConsoleSupportsAnsi()
        {
            if (Console.IsOutputRedirected) return false;
            const int STD_OUTPUT_HANDLE = -11;
            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (handle == new IntPtr(-1)) return false;
            if (!GetConsoleMode(handle, out uint mode)) return false;
            return (mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != 0 ||
                SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        internal static void WriteCategorizedHelp(RootCommand root, Dictionary<string, List<Option>> categorized)
    {
            TextWriter output = Console.Out;
                        bool useColor = ConsoleSupportsAnsi();
                        string cBold = useColor ? CBold : "";
            string cDim = useColor ? CDim : "";
            string cReset = useColor ? CReset : "";
            string cCyan = useColor ? CCyan : "";
            string cMagenta = useColor ? "\x1b[35m" : "";
            int maxWidth;
            try { maxWidth = Console.WindowWidth; } catch (IOException) { maxWidth = 120; }
            if (maxWidth <= 0) maxWidth = 120;

            // Description
            output.WriteLine($"{cBold}{root.Description}{cReset}");
            output.WriteLine();
            output.WriteLine($"{cBold}Usage:{cReset}");
        output.WriteLine($"  BgRaster [options]");
        output.WriteLine();

        string[] catOrder = ["Frequent", "Advanced", "Appearance"];
        int maxOptionLen = 0;
        foreach (var cat in catOrder)
        {
            if (!categorized.TryGetValue(cat, out var opts)) continue;
            foreach (var opt in opts)
            {
                string name = GetOptionDisplayName(opt);
                if (name.Length > maxOptionLen) maxOptionLen = name.Length;
            }
                    }
                    // Built-in options
                    maxOptionLen = Math.Max(maxOptionLen, "-?, -h, --help".Length);
                    maxOptionLen = Math.Max(maxOptionLen, "--version".Length);
                    maxOptionLen = Math.Min(maxOptionLen + 2, 62);

        foreach (string cat in catOrder)
        {
            if (!categorized.TryGetValue(cat, out var opts) || opts.Count == 0) continue;

            output.WriteLine();
            output.WriteLine($"  {cBold}{cCyan}{cat} options:{cReset}");

            foreach (var opt in opts)
            {
                string name = GetOptionDisplayName(opt);
                string desc = opt.Description ?? "";
                string pad = name.Length < maxOptionLen
                    ? new string(' ', maxOptionLen - name.Length)
                    : " ";
                output.WriteLine($"  {name}{pad}{desc}");
            }
        }

        output.WriteLine();
                string[] builtins = ["-?, -h, --help", "--version"];
                        string[] builtinDescs = ["Show help and usage information", "Show version information"];
                        for (int i = 0; i < builtins.Length; i++)
                        {
                            string name = builtins[i];
                            string pad = name.Length < maxOptionLen
                                ? new string(' ', maxOptionLen - name.Length)
                                : " ";
                            output.WriteLine($"  {name}{pad}{cMagenta}{builtinDescs[i]}{cReset}");
                        }

                        output.WriteLine();
                        output.WriteLine($"{cBold}{cCyan}Examples:{cReset}");
                        output.WriteLine($"{cMagenta}  BgRaster --config ./wallpaper.toml --render-output ./out/ --text-format \"Hello from BgRaster\"{cReset}");
                                }

    private static string GetOptionDisplayName(Option opt)
    {
        string name = "--" + opt.Name.TrimStart('-');
        // Check for value syntax from CliOptionDefinition
        var def = GeneratedCliOptionCatalog.Definitions.FirstOrDefault(
            d => string.Equals("--" + d.Alias.TrimStart('-'), opt.Name, StringComparison.OrdinalIgnoreCase)
              || string.Equals(d.Alias, opt.Name, StringComparison.OrdinalIgnoreCase));
        if (def?.ValueSyntax is { } vs)
            return $"{name} {vs}";
        return name;
    }

    internal static string GetVersionString()
    {
        string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";
        return $"BgRaster {version} {GetCopyright()}";
    }

    internal static string GetCopyright()
    {
        return Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyCopyrightAttribute>()
            ?.Copyright ?? "(C) 2026 Barjonas LLC";
    }
}