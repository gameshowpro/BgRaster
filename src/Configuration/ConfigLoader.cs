// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

static class ConfigLoader
{
    internal static GlobalOptions Load(string path, List<string>? warnings = null)
    {
        return Load(path, out _, warnings);
    }

    internal static GlobalOptions Load(string path, out HashSet<string> tomlPaths, List<string>? warnings = null)
    {
        string toml = File.ReadAllText(path);
        string configDirectory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? Directory.GetCurrentDirectory();
        TomlTable table;
        try
        {
            table = Toml.ToModel(toml);
        }
        catch (TomlException ex)
        {
            throw new FormatException($"Failed to parse TOML config '{path}': {ex.Message}", ex);
        }

        tomlPaths = TomlKeyCollector.Collect(table);
        return ParseGlobalOptions(table, warnings, configDirectory);
    }

    internal static GlobalOptions ApplyCliOverlay(GlobalOptions options, CliOverlay overlay, List<string>? warnings = null)
    {
        return ApplyCliOverlay(options, overlay, out _, warnings);
    }

    internal static GlobalOptions ApplyCliOverlay(GlobalOptions options, CliOverlay overlay, out HashSet<string> cliPaths, List<string>? warnings = null)
    {
        cliPaths = new HashSet<string>(StringComparer.Ordinal);
        string currentDirectory = Directory.GetCurrentDirectory();

        TextOptions text = options.Text;
        BackgroundOptions bg = options.Background;
        GridOptions grid = options.Grid;
        CircleOptions circle = options.Circle;
        CrosshairOptions crosshair = options.Crosshair;
        LogoOptions logo = options.Logo;
        RenderOptions render = options.Render;

        if (overlay.MachineName is not null)
        { render = render with { MachineName = overlay.MachineName }; cliPaths.Add("render.machine-name"); }

        if (overlay.TextFormat is not null)
        { text = text with { Format = ParseCliStringOrArray(overlay.TextFormat, "cli --text-format", warnings) }; cliPaths.Add("text.format"); }
        if (overlay.TextSize is not null)
        { text = text with { Size = ParseCliStringOrArray(overlay.TextSize, "cli --text-size", warnings) }; cliPaths.Add("text.size"); }
        if (overlay.TextColor is not null)
        { text = text with { Color = ParseCliStringOrArray(overlay.TextColor, "cli --text-color", warnings) }; cliPaths.Add("text.color"); }
        if (overlay.TextX is not null)
        { text = text with { X = ParseCliStringOrArray(overlay.TextX, "cli --text-x", warnings) }; cliPaths.Add("text.x"); }
        if (overlay.TextY is not null)
        { text = text with { Y = ParseCliStringOrArray(overlay.TextY, "cli --text-y", warnings) }; cliPaths.Add("text.y"); }

        if (overlay.BackgroundColor is not null)
        { bg = bg with { Color = ParseCliStringOrArray(overlay.BackgroundColor, "cli --background-color", warnings) }; cliPaths.Add("background.color"); }
        if (overlay.BackgroundImage is not null)
        {
            bg = bg with
            {
                Image = ResolveCliPathArray(
                    ParseCliStringOrArray(overlay.BackgroundImage, "cli --background-image", warnings),
                    currentDirectory),
            };
            cliPaths.Add("background.image");
        }
        if (overlay.BackgroundFit is not null)
        { bg = bg with { Fit = ParseCliStringOrArray(overlay.BackgroundFit, "cli --background-fit", warnings) }; cliPaths.Add("background.fit"); }
        if (overlay.BackgroundAlternating is not null) { bg = bg with { Alternating = [overlay.BackgroundAlternating.Value] }; cliPaths.Add("background.alternating"); }
        if (overlay.BackgroundBorder is not null) { bg = bg with { Border = [overlay.BackgroundBorder.Value] }; cliPaths.Add("background.border"); }
        if (overlay.BackgroundBorderColor is not null)
        { bg = bg with { BorderColor = ParseCliStringOrArray(overlay.BackgroundBorderColor, "cli --background-border-color", warnings) }; cliPaths.Add("background.border-color"); }

        if (overlay.GridSize is not null)
        { grid = grid with { Size = ParseCliStringOrArray(overlay.GridSize, "cli --grid-size", warnings) }; cliPaths.Add("grid.size"); }
        if (overlay.GridOddColor is not null)
        { grid = grid with { OddColor = ParseCliStringOrArray(overlay.GridOddColor, "cli --grid-odd-color", warnings) }; cliPaths.Add("grid.odd-color"); }
        if (overlay.GridEvenColor is not null)
        { grid = grid with { EvenColor = ParseCliStringOrArray(overlay.GridEvenColor, "cli --grid-even-color", warnings) }; cliPaths.Add("grid.even-color"); }
        if (overlay.GridStroke is not null)
        { grid = grid with { Stroke = ParseCliStringOrArray(overlay.GridStroke, "cli --grid-stroke", warnings) }; cliPaths.Add("grid.stroke"); }
        if (overlay.GridOffsetX is not null)
        { grid = grid with { OffsetX = ParseCliStringOrArray(overlay.GridOffsetX, "cli --grid-offset-x", warnings) }; cliPaths.Add("grid.offset-x"); }
        if (overlay.GridOffsetY is not null)
        { grid = grid with { OffsetY = ParseCliStringOrArray(overlay.GridOffsetY, "cli --grid-offset-y", warnings) }; cliPaths.Add("grid.offset-y"); }
        if (overlay.GridCoordinates is not null) { grid = grid with { Coordinates = [overlay.GridCoordinates.Value] }; cliPaths.Add("grid.coordinates"); }

        if (overlay.CircleSize is not null)
        { circle = circle with { Size = ParseCliStringOrArray(overlay.CircleSize, "cli --circle-size", warnings) }; cliPaths.Add("circle.size"); }
        if (overlay.CircleX is not null)
        { circle = circle with { X = ParseCliStringOrArray(overlay.CircleX, "cli --circle-x", warnings) }; cliPaths.Add("circle.x"); }
        if (overlay.CircleY is not null)
        { circle = circle with { Y = ParseCliStringOrArray(overlay.CircleY, "cli --circle-y", warnings) }; cliPaths.Add("circle.y"); }
        if (overlay.CircleColor is not null)
        { circle = circle with { Color = ParseCliStringOrArray(overlay.CircleColor, "cli --circle-color", warnings) }; cliPaths.Add("circle.color"); }
        if (overlay.CircleStroke is not null)
        { circle = circle with { Stroke = ParseCliStringOrArray(overlay.CircleStroke, "cli --circle-stroke", warnings) }; cliPaths.Add("circle.stroke"); }

        if (overlay.CrosshairLength is not null)
        { crosshair = crosshair with { Length = ParseCliStringOrArray(overlay.CrosshairLength, "cli --crosshair-length", warnings) }; cliPaths.Add("crosshair.length"); }
        if (overlay.CrosshairX is not null)
        { crosshair = crosshair with { X = ParseCliStringOrArray(overlay.CrosshairX, "cli --crosshair-x", warnings) }; cliPaths.Add("crosshair.x"); }
        if (overlay.CrosshairY is not null)
        { crosshair = crosshair with { Y = ParseCliStringOrArray(overlay.CrosshairY, "cli --crosshair-y", warnings) }; cliPaths.Add("crosshair.y"); }
        if (overlay.CrosshairColor is not null)
        { crosshair = crosshair with { Color = ParseCliStringOrArray(overlay.CrosshairColor, "cli --crosshair-color", warnings) }; cliPaths.Add("crosshair.color"); }
        if (overlay.CrosshairStroke is not null)
        { crosshair = crosshair with { Stroke = ParseCliStringOrArray(overlay.CrosshairStroke, "cli --crosshair-stroke", warnings) }; cliPaths.Add("crosshair.stroke"); }

        if (overlay.LogoSource is not null)
        {
            logo = logo with
            {
                Source = ResolveCliPathArray(
                    ParseCliStringOrArray(overlay.LogoSource, "cli --logo-source", warnings),
                    currentDirectory),
            };
            cliPaths.Add("logo.source");
        }
        if (overlay.LogoX is not null)
        { logo = logo with { X = ParseCliStringOrArray(overlay.LogoX, "cli --logo-x", warnings) }; cliPaths.Add("logo.x"); }
        if (overlay.LogoY is not null)
        { logo = logo with { Y = ParseCliStringOrArray(overlay.LogoY, "cli --logo-y", warnings) }; cliPaths.Add("logo.y"); }
        if (overlay.LogoAnchorX is not null)
        { logo = logo with { AnchorX = overlay.LogoAnchorX }; cliPaths.Add("logo.anchor-x"); }
        if (overlay.LogoAnchorY is not null)
        { logo = logo with { AnchorY = overlay.LogoAnchorY }; cliPaths.Add("logo.anchor-y"); }
        if (overlay.LogoWidth is not null)
        { logo = logo with { Width = ParseCliStringOrArray(overlay.LogoWidth, "cli --logo-width", warnings) }; cliPaths.Add("logo.width"); }
        if (overlay.LogoHeight is not null)
        { logo = logo with { Height = ParseCliStringOrArray(overlay.LogoHeight, "cli --logo-height", warnings) }; cliPaths.Add("logo.height"); }
        if (overlay.LogoOpacity is not null)
        { logo = logo with { Opacity = ParseCliFloatOrArray(overlay.LogoOpacity, "cli --logo-opacity") }; cliPaths.Add("logo.opacity"); }

        if (overlay.RenderDryRun is not null) { render = render with { DryRun = overlay.RenderDryRun.Value }; cliPaths.Add("render.no-assignment"); }
        if (overlay.RenderNoDiscovery is not null) { render = render with { NoDiscovery = overlay.RenderNoDiscovery.Value }; cliPaths.Add("render.no-discovery"); }
        if (overlay.RenderOutputsSkipUnspecified is not null)
        { render = render with { OutputsSkipUnspecified = overlay.RenderOutputsSkipUnspecified.Value }; cliPaths.Add("render.outputs-skip-unspecified"); }
        if (overlay.RenderOutput is not null)
        { render = render with { Output = ResolveCliPath(overlay.RenderOutput, currentDirectory) }; cliPaths.Add("render.output"); }
        if (overlay.RenderVerbosity is not null)
        {
            render = render with
            {
                MinimumLogLevel = ParseLogLevel(
                    overlay.RenderVerbosity,
                    "cli --verbosity",
                    warnings),
            };
            cliPaths.Add("render.verbosity");
        }
        NetworkOptions network = options.Network;
        if (overlay.NetworkRequireAdapterType is not null)
        { network = network with { RequireAdapterType = ParseCliStringOrArray(overlay.NetworkRequireAdapterType, "cli --network-require-adapter-type", warnings) }; cliPaths.Add("network.require-adapter-type"); }
        if (overlay.NetworkRequireUp is not null) { network = network with { RequireUp = overlay.NetworkRequireUp.Value }; cliPaths.Add("network.require-up"); }
        if (overlay.NetworkRequireFamily is not null) { network = network with { RequireFamily = overlay.NetworkRequireFamily }; cliPaths.Add("network.require-family"); }
        if (overlay.NetworkAdapterFormat is not null) { network = network with { AdapterFormat = overlay.NetworkAdapterFormat }; cliPaths.Add("network.adapter-format"); }
        if (overlay.NetworkIpAddressFormat is not null) { network = network with { IpAddressFormat = overlay.NetworkIpAddressFormat }; cliPaths.Add("network.ip-address-format"); }
        if (overlay.NetworkX is not null) { network = network with { X = ParseCliStringOrArray(overlay.NetworkX, "cli --network-x", warnings) }; cliPaths.Add("network.x"); }
        if (overlay.NetworkY is not null) { network = network with { Y = ParseCliStringOrArray(overlay.NetworkY, "cli --network-y", warnings) }; cliPaths.Add("network.y"); }
        if (overlay.NetworkSize is not null) { network = network with { Size = ParseCliStringOrArray(overlay.NetworkSize, "cli --network-size", warnings) }; cliPaths.Add("network.size"); }
        if (overlay.NetworkColor is not null) { network = network with { Color = ParseCliStringOrArray(overlay.NetworkColor, "cli --network-color", warnings) }; cliPaths.Add("network.color"); }
        if (overlay.NetworkRender is not null) { network = network with { Render = overlay.NetworkRender.Value }; cliPaths.Add("network.render"); }

        if (overlay.RenderContinueAfterUnchanged is not null)
        { render = render with { ContinueAfterUnchanged = overlay.RenderContinueAfterUnchanged.Value }; cliPaths.Add("render.force"); }

        return options with
        {
            Text = text,
            Background = bg,
            Grid = grid,
            Circle = circle,
            Crosshair = crosshair,
            Logo = logo,
            Render = render,
            Network = network,
        };
    }

    static GlobalOptions ParseGlobalOptions(TomlTable table, List<string>? warnings, string configDirectory)
    {

        TextOptions text = table.TryGetValue("text", out object? textObj) && textObj is TomlTable textTable
            ? ParseTextOptions(textTable)
            : new TextOptions();

        BackgroundOptions background = table.TryGetValue("background", out object? bgObj) && bgObj is TomlTable bgTable
            ? ParseBackgroundOptions(bgTable, configDirectory)
            : new BackgroundOptions();

        GridOptions grid = table.TryGetValue("grid", out object? gridObj) && gridObj is TomlTable gridTable
            ? ParseGridOptions(gridTable)
            : new GridOptions();

        CircleOptions circle = table.TryGetValue("circle", out object? circleObj) && circleObj is TomlTable circleTable
            ? ParseCircleOptions(circleTable)
            : new CircleOptions();

        CrosshairOptions crosshair = table.TryGetValue("crosshair", out object? crosshairObj) && crosshairObj is TomlTable crosshairTable
            ? ParseCrosshairOptions(crosshairTable)
            : new CrosshairOptions();

        LabeledEdgesOptions labeledEdges = table.TryGetValue("labeled-edges", out object? labeledEdgesObj) && labeledEdgesObj is TomlTable labeledEdgesTable
            ? ParseLabeledEdgesOptions(labeledEdgesTable)
            : new LabeledEdgesOptions();

        LogoOptions logo = table.TryGetValue("logo", out object? logoObj) && logoObj is TomlTable logoTable
            ? ParseLogoOptions(logoTable, configDirectory)
            : new LogoOptions();

        RenderOptions render = table.TryGetValue("render", out object? renderObj) && renderObj is TomlTable renderTable
            ? ParseRenderOptions(renderTable, warnings, configDirectory)
            : new RenderOptions();

        NetworkOptions network = table.TryGetValue("network", out object? networkObj) && networkObj is TomlTable networkTable
            ? ParseNetworkOptions(networkTable)
            : new NetworkOptions();

        ImmutableArray<OutputOptions> outputs = table.TryGetValue("output", out object? outputObj) && outputObj is TomlTableArray outputArray
            ? [.. outputArray.Select(outputTable => ParseOutputOptions(outputTable, configDirectory))]
            : [];

        return new GlobalOptions
        {
            Text = text,
            Background = background,
            Grid = grid,
            Circle = circle,
            Crosshair = crosshair,
            LabeledEdges = labeledEdges,
            Logo = logo,
            Render = render,
            Network = network,
            Outputs = outputs,
        };
    }

    static TextOptions ParseTextOptions(TomlTable t) => new()
    {
        Format = GetStringArray(t, "format") ?? ParseLegacyText(t) ?? new TextOptions().Format,
        TextAlign = GetString(t, "text-align") ?? GetString(t, "textAlign") ?? GetString(t, "justify") ?? new TextOptions().TextAlign,
        AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX") ?? new TextOptions().AnchorX,
        AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY") ?? new TextOptions().AnchorY,
        Size = GetStringArray(t, "size") ?? new TextOptions().Size,
        Color = GetStringArray(t, "color") ?? new TextOptions().Color,
        X = GetStringArray(t, "x") ?? new TextOptions().X,
        Y = GetStringArray(t, "y") ?? new TextOptions().Y,
    };

    static BackgroundOptions ParseBackgroundOptions(TomlTable t, string configDirectory) => new()
    {
        Color = GetStringArray(t, "color") ?? new BackgroundOptions().Color,
        Image = ResolveConfigRelativePathArray(GetStringArray(t, "image"), configDirectory) ?? new BackgroundOptions().Image,
        Fit = GetStringArray(t, "fit") ?? new BackgroundOptions().Fit,
        Alternating = GetBoolArray(t, "alternating") ?? new BackgroundOptions().Alternating,
        Border = GetBoolArray(t, "border") ?? new BackgroundOptions().Border,
        BorderColor = GetStringArray(t, "border-color") ?? new BackgroundOptions().BorderColor,
    };

    static GridOptions ParseGridOptions(TomlTable t) => new()
    {
        Size = GetStringArray(t, "size") ?? new GridOptions().Size,
        OddColor = GetStringArray(t, "odd-color") ?? new GridOptions().OddColor,
        EvenColor = GetStringArray(t, "even-color") ?? new GridOptions().EvenColor,
        Stroke = GetStringArray(t, "stroke") ?? new GridOptions().Stroke,
        OffsetX = GetStringArray(t, "offset-x") ?? new GridOptions().OffsetX,
        OffsetY = GetStringArray(t, "offset-y") ?? new GridOptions().OffsetY,
        Coordinates = GetBoolArray(t, "coordinates") ?? new GridOptions().Coordinates,
    };

    static CircleOptions ParseCircleOptions(TomlTable t) => new()
    {
        X = GetStringArray(t, "x") ?? new CircleOptions().X,
        Y = GetStringArray(t, "y") ?? new CircleOptions().Y,
        Size = GetStringArray(t, "size") ?? new CircleOptions().Size,
        Color = GetStringArray(t, "color") ?? new CircleOptions().Color,
        Stroke = GetStringArray(t, "stroke") ?? new CircleOptions().Stroke,
    };

    static CrosshairOptions ParseCrosshairOptions(TomlTable t) => new()
    {
        X = GetStringArray(t, "x") ?? new CrosshairOptions().X,
        Y = GetStringArray(t, "y") ?? new CrosshairOptions().Y,
        Length = GetStringArray(t, "length") ?? new CrosshairOptions().Length,
        Color = GetStringArray(t, "color") ?? new CrosshairOptions().Color,
        Stroke = GetStringArray(t, "stroke") ?? new CrosshairOptions().Stroke,
    };

    static LabeledEdgesOptions ParseLabeledEdgesOptions(TomlTable t) => new()
    {
        TextSize = GetStringArray(t, "text-size") ?? new LabeledEdgesOptions().TextSize,
        TailLength = GetStringArray(t, "tail-length") ?? new LabeledEdgesOptions().TailLength,
        Thickness = GetStringArray(t, "thickness") ?? new LabeledEdgesOptions().Thickness,
        HeadScale = GetFloatArrayRequiredRange(t, "head-scale", "config [labeled-edges].head-scale", minInclusive: 0f, maxInclusive: float.MaxValue) ?? new LabeledEdgesOptions().HeadScale,
        Scope = GetStringArray(t, "scope") is ImmutableArray<string> scopeValues ? ParseLabeledEdgesScopes(scopeValues, "config [labeled-edges].scope") : new LabeledEdgesOptions().Scope,
        Side = GetStringArray(t, "side") is ImmutableArray<string> sideValues ? ParseLabeledEdgeSides(sideValues, "config [labeled-edges].side") : new LabeledEdgesOptions().Side,
    };

    static LogoOptions ParseLogoOptions(TomlTable t, string configDirectory) => new()
    {
        Source = ResolveConfigRelativePathArray(GetStringArray(t, "source"), configDirectory) ?? new LogoOptions().Source,
        X = GetStringArray(t, "x") ?? new LogoOptions().X,
        Y = GetStringArray(t, "y") ?? new LogoOptions().Y,
        AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX") ?? new LogoOptions().AnchorX,
        AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY") ?? new LogoOptions().AnchorY,
        Width = GetStringArray(t, "width") ?? new LogoOptions().Width,
        Height = GetStringArray(t, "height") ?? new LogoOptions().Height,
        Opacity = GetFloatArrayRequiredRange(t, "opacity", "config [logo].opacity", minInclusive: 0f, maxInclusive: 1f) ?? new LogoOptions().Opacity,
    };

    static RenderOptions ParseRenderOptions(TomlTable t, List<string>? warnings, string configDirectory)
    {
        string? verbosityText = GetString(t, "verbosity");

        return new RenderOptions
        {
            DryRun = GetBool(t, "no-assignment") ?? false,
            NoDiscovery = GetBool(t, "no-discovery") ?? false,
            OutputsSkipUnspecified = GetBool(t, "outputs-skip-unspecified") ?? false,
            Output = ResolveConfigRelativePath(GetString(t, "output") ?? string.Empty, configDirectory),
            MinimumLogLevel = ParseLogLevel(verbosityText, "config [render].verbosity", warnings),
            ContinueAfterUnchanged = GetBool(t, "force") ?? GetBool(t, "render-force") ?? false,
            MachineName = GetString(t, "machine-name") ?? new RenderOptions().MachineName,
            SimulateNetwork = GetBool(t, "simulate-network") ?? new RenderOptions().SimulateNetwork,
        };
    }

    static OutputOptions ParseOutputOptions(TomlTable t, string configDirectory)
    {
        OutputTarget target = t.TryGetValue("target", out object? targetObj)
            ? targetObj switch
            {
                long i => OutputTarget.FromIndex((int)i),
                string s => OutputTarget.FromId(s),
                _ => OutputTarget.FromIndex(0),
            }
            : OutputTarget.FromIndex(0);

        ImmutableArray<SliceOptions> slices = t.TryGetValue("slice", out object? sliceObj) && sliceObj is TomlTableArray sliceArray
            ? [.. sliceArray.Select(sliceTable => ParseSliceOptions(sliceTable, configDirectory))]
            : [];

        return new OutputOptions
        {
            Target = target,
            HardwareOutput = ParseHardwareOutputReference(t),
            Text = ParseTextOverride(t, "text"),
            Background = ParseBackgroundOverride(t, "background", configDirectory),
            Grid = ParseGridOverride(t, "grid"),
            Circle = ParseCircleOverride(t, "circle"),
            Crosshair = ParseCrosshairOverride(t, "crosshair"),
            LabeledEdges = ParseLabeledEdgesOverride(t, "labeled-edges"),
            Logo = ParseLogoOverride(t, "logo", configDirectory),
            Network = ParseNetworkOverride(t, "network"),
            Slices = slices,
        };
    }

    static OutputRecord? ParseHardwareOutputReference(TomlTable parent)
    {
        if (!parent.TryGetValue("hardware_output", out object? hardwareObject) || hardwareObject is not TomlTable hardwareTable)
            return null;

        string id = GetString(hardwareTable, "id") ?? string.Empty;
        int index = hardwareTable.TryGetValue("index", out object? indexObject) && indexObject is long indexValue
            ? (int)indexValue
            : 0;

        int desktopX = hardwareTable.TryGetValue("desktopX", out object? desktopXObject) && desktopXObject is long desktopXValue
            ? (int)desktopXValue
            : 0;
        int desktopY = hardwareTable.TryGetValue("desktopY", out object? desktopYObject) && desktopYObject is long desktopYValue
            ? (int)desktopYValue
            : 0;
        int widthPx = hardwareTable.TryGetValue("widthPx", out object? widthObject) && widthObject is long widthValue
            ? (int)widthValue
            : 0;
        int heightPx = hardwareTable.TryGetValue("heightPx", out object? heightObject) && heightObject is long heightValue
            ? (int)heightValue
            : 0;
        int dpiX = hardwareTable.TryGetValue("dpiX", out object? dpiXObject) && dpiXObject is long dpiXValue
            ? (int)dpiXValue
            : 0;
        int dpiY = hardwareTable.TryGetValue("dpiY", out object? dpiYObject) && dpiYObject is long dpiYValue
            ? (int)dpiYValue
            : 0;
        int rotation = hardwareTable.TryGetValue("rotation", out object? rotationObject) && rotationObject is long rotationValue
            ? (int)rotationValue
            : 0;

        return new OutputRecord
        {
            Id = id,
            Index = index,
            DesktopX = desktopX,
            DesktopY = desktopY,
            WidthPx = widthPx,
            HeightPx = heightPx,
            DpiX = dpiX,
            DpiY = dpiY,
            Rotation = rotation,
            AdapterName = GetString(hardwareTable, "adapterName") ?? string.Empty,
            FriendlyName = GetString(hardwareTable, "friendlyName") ?? string.Empty,
        };
    }

    static SliceOptions ParseSliceOptions(TomlTable t, string configDirectory) => new()
    {
        X = GetString(t, "x") ?? "0",
        Y = GetString(t, "y") ?? "0",
        Width = GetString(t, "width") ?? "100vw",
        Height = GetString(t, "height") ?? "100vh",
        Text = ParseTextOverride(t, "text"),
        Background = ParseBackgroundOverride(t, "background", configDirectory),
        Grid = ParseGridOverride(t, "grid"),
        Circle = ParseCircleOverride(t, "circle"),
        Crosshair = ParseCrosshairOverride(t, "crosshair"),
        LabeledEdges = ParseLabeledEdgesOverride(t, "labeled-edges"),
        Logo = ParseLogoOverride(t, "logo", configDirectory),
        Network = ParseNetworkOverride(t, "network"),
    };

    static TextOverride? ParseTextOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;

        ImmutableArray<string>? formatOverride = GetStringArray(t, "format") ?? ParseLegacyText(t);

        return new TextOverride
        {
            Format = formatOverride,
            TextAlign = GetString(t, "text-align") ?? GetString(t, "textAlign") ?? GetString(t, "justify"),
            AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX"),
            AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY"),
            Size = GetStringArray(t, "size"),
            Color = GetStringArray(t, "color"),
            X = GetString(t, "x"),
            Y = GetString(t, "y"),
        };
    }

    static ImmutableArray<string>? ParseLegacyText(TomlTable t)
    {
        ImmutableArray<string>? title = GetStringArray(t, "title");
        ImmutableArray<string>? subtitle = GetStringArray(t, "subtitle");

        if (title is null && subtitle is null)
            return null;

        string[] defaults = [.. new TextOptions().Format];
        defaults[0] = title is { Length: > 0 } ? title.Value[0] : defaults[0];
        defaults[2] = subtitle is { Length: > 0 } ? subtitle.Value[0] : defaults[2];
        return [.. defaults];
    }

    static ImmutableArray<string> ParseCliStringOrArray(string raw, string source, List<string>? warnings)
    {
        string trimmed = raw.Trim();
        if (!trimmed.StartsWith('['))
            return [raw];

        try
        {
            TomlTable model = Toml.ToModel($"value = {trimmed}");
            if (model.TryGetValue("value", out object? value) && value is TomlArray array)
            {
                bool allStrings = array.All(item => item is string);
                if (allStrings)
                    return [.. array.Cast<string>()];

                warnings?.Add($"Invalid CLI string-array '{raw}' from {source}; expected all string elements. Treating as literal string.");
                return [raw];
            }
        }
        catch (Exception)
        {
            warnings?.Add($"Invalid CLI string-array '{raw}' from {source}; treating as literal string.");
        }

        return [raw];
    }

    static ImmutableArray<string> ResolveCliPathArray(ImmutableArray<string> values, string baseDirectory)
    {
        return ConfiguredPathResolver.ResolveArray(values, baseDirectory) ?? values;
    }

    static string ResolveCliPath(string value, string baseDirectory)
    {
        return ConfiguredPathResolver.Resolve(value, baseDirectory);
    }

    static ImmutableArray<float> ParseCliFloatOrArray(string raw, string source)
    {
        string trimmed = raw.Trim();
        if (!trimmed.StartsWith('['))
        {
            if (!float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float singleValue))
                throw new FormatException($"Invalid numeric value '{raw}' from {source}; expected a float or TOML float array literal.");

            ValidateRange(singleValue, source);
            return [singleValue];
        }

        TomlTable model;
        try
        {
            model = Toml.ToModel($"value = {trimmed}");
        }
        catch (Exception ex)
        {
            throw new FormatException($"Invalid TOML float array literal '{raw}' from {source}; expected format like [0.5, 1.0].", ex);
        }

        if (!model.TryGetValue("value", out object? value) || value is not TomlArray array)
            throw new FormatException($"Invalid numeric value '{raw}' from {source}; expected a float or TOML float array literal.");

        ImmutableArray<float>.Builder values = ImmutableArray.CreateBuilder<float>(array.Count);
        for (int i = 0; i < array.Count; i++)
        {
            if (!TryConvertTomlNumber(array[i], out float parsed))
                throw new FormatException($"Invalid numeric value at index {i} in {source}: '{array[i]}'; expected a float.");

            ValidateRange(parsed, source);
            values.Add(parsed);
        }

        return values.ToImmutable();

        static void ValidateRange(float value, string source)
        {
            if (value < 0f || value > 1f)
                throw new FormatException($"Value {value.ToString(System.Globalization.CultureInfo.InvariantCulture)} from {source} must be within [0, 1].");
        }
    }

    static BackgroundOverride? ParseBackgroundOverride(TomlTable parent, string key, string configDirectory)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new BackgroundOverride
        {
            Color = GetString(t, "color"),
            Image = ResolveConfigRelativePath(GetString(t, "image"), configDirectory),
            Fit = GetString(t, "fit"),
            Alternating = GetBool(t, "alternating"),
            Border = GetBool(t, "border"),
            BorderColor = GetString(t, "border-color"),
        };
    }

    static GridOverride? ParseGridOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new GridOverride
        {
            Size = GetString(t, "size"),
            OddColor = GetString(t, "odd-color"),
            EvenColor = GetString(t, "even-color"),
            Stroke = GetString(t, "stroke"),
            OffsetX = GetString(t, "offset-x"),
            OffsetY = GetString(t, "offset-y"),
            Coordinates = GetBool(t, "coordinates"),
        };
    }

    static CircleOverride? ParseCircleOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new CircleOverride
        {
            X = GetString(t, "x"),
            Y = GetString(t, "y"),
            Size = GetString(t, "size"),
            Color = GetString(t, "color"),
            Stroke = GetString(t, "stroke"),
        };
    }

    static CrosshairOverride? ParseCrosshairOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new CrosshairOverride
        {
            X = GetString(t, "x"),
            Y = GetString(t, "y"),
            Length = GetString(t, "length"),
            Color = GetString(t, "color"),
            Stroke = GetString(t, "stroke"),
        };
    }

    static LabeledEdgesOverride? ParseLabeledEdgesOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;

        ImmutableArray<LabeledEdgeSide>? sides = null;
        if (GetStringArray(t, "side") is ImmutableArray<string> sideValues)
            sides = ParseLabeledEdgeSides(sideValues, $"config [{key}].side");

        return new LabeledEdgesOverride
        {
            TextSize = GetString(t, "text-size"),
            TailLength = GetString(t, "tail-length"),
            Thickness = GetString(t, "thickness"),
            HeadScale = GetFloatRequiredRange(t, "head-scale", $"config [{key}].head-scale", minInclusive: 0f, maxInclusive: float.MaxValue),
            Scope = GetString(t, "scope"),
            Side = sides,
        };
    }

    static NetworkOptions ParseNetworkOptions(TomlTable t) => new()
    {
        RequireAdapterType = GetStringArray(t, "require_adapter_type") ?? new NetworkOptions().RequireAdapterType,
        ExcludeAdapterType = GetStringArray(t, "exclude_adapter_type") ?? new NetworkOptions().ExcludeAdapterType,
        RequireUp = GetBool(t, "require_up") ?? true,
        RequireFamily = GetString(t, "require_family") ?? "IPv4",
        RequireMacAddress = GetStringArray(t, "require_mac_address") ?? new NetworkOptions().RequireMacAddress,
        RequireSubnet = GetStringArray(t, "require_subnet") ?? new NetworkOptions().RequireSubnet,
        MinimumAddressCount = GetInt(t, "minimum_address_count") ?? new NetworkOptions().MinimumAddressCount,
        RequireName = GetStringArray(t, "require_name") ?? new NetworkOptions().RequireName,
        RequireDescription = GetStringArray(t, "require_description") ?? new NetworkOptions().RequireDescription,
        IpAddressFormat = GetString(t, "ip_address_format") ?? new NetworkOptions().IpAddressFormat,
        AdapterFormat = GetString(t, "adapter_format") ?? new NetworkOptions().AdapterFormat,
        TextAlign = GetString(t, "text-align") ?? GetString(t, "textAlign") ?? GetString(t, "justify") ?? new NetworkOptions().TextAlign,
        AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX") ?? new NetworkOptions().AnchorX,
        AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY") ?? new NetworkOptions().AnchorY,
        X = GetStringArray(t, "x") ?? new NetworkOptions().X,
        Y = GetStringArray(t, "y") ?? new NetworkOptions().Y,
        Size = GetStringArray(t, "size") ?? new NetworkOptions().Size,
        Color = GetStringArray(t, "color") ?? new NetworkOptions().Color,
        Render = GetBool(t, "render") ?? new NetworkOptions().Render,
    };

    static NetworkOverride? ParseNetworkOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new NetworkOverride
        {
            RequireAdapterType = GetStringArray(t, "require_adapter_type"),
            ExcludeAdapterType = GetStringArray(t, "exclude_adapter_type"),
            RequireUp = GetBool(t, "require_up"),
            RequireFamily = GetString(t, "require_family"),
            RequireMacAddress = GetStringArray(t, "require_mac_address"),
            RequireSubnet = GetStringArray(t, "require_subnet"),
            MinimumAddressCount = GetInt(t, "minimum_address_count"),
            RequireName = GetStringArray(t, "require_name"),
            RequireDescription = GetStringArray(t, "require_description"),
            IpAddressFormat = GetString(t, "ip_address_format"),
            AdapterFormat = GetString(t, "adapter_format"),
            TextAlign = GetString(t, "text-align") ?? GetString(t, "textAlign") ?? GetString(t, "justify"),
            AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX"),
            AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY"),
            X = GetStringArray(t, "x"),
            Y = GetStringArray(t, "y"),
            Size = GetStringArray(t, "size"),
            Color = GetStringArray(t, "color"),
            Render = GetBool(t, "render"),
        };
    }

    static ImmutableArray<LabeledEdgeSide> ParseLabeledEdgeSides(ImmutableArray<string> values, string source)
    {
        HashSet<LabeledEdgeSide> seen = [];
        ImmutableArray<LabeledEdgeSide>.Builder sides = ImmutableArray.CreateBuilder<LabeledEdgeSide>(values.Length);

        for (int index = 0; index < values.Length; index++)
        {
            string raw = values[index].Trim();
            if (!TryParseLabeledEdgeSide(raw, out LabeledEdgeSide side))
                throw new FormatException($"Invalid labeled-edge side value '{raw}' at index {index} in {source}; expected one of TL, T, TR, R, BR, B, BL, L.");

            if (!seen.Add(side))
                throw new FormatException($"Duplicate labeled-edge side '{raw}' in {source}; each side may appear at most once.");

            sides.Add(side);
        }

        return sides.ToImmutable();
    }

    static ImmutableArray<LabeledEdgesScope> ParseLabeledEdgesScopes(ImmutableArray<string> values, string source)
    {
        ImmutableArray<LabeledEdgesScope>.Builder scopes = ImmutableArray.CreateBuilder<LabeledEdgesScope>(values.Length);
        for (int index = 0; index < values.Length; index++)
        {
            string raw = values[index].Trim();
            if (!TryParseLabeledEdgesScope(raw, out LabeledEdgesScope scope))
                throw new FormatException($"Invalid labeled-edge scope value '{raw}' at index {index} in {source}; expected one of Desktop, Output, Slice.");

            scopes.Add(scope);
        }

        return scopes.ToImmutable();
    }

    static bool TryParseLabeledEdgeSide(string raw, out LabeledEdgeSide side)
    {
        side = raw switch
        {
            "TL" => LabeledEdgeSide.TL,
            "T" => LabeledEdgeSide.T,
            "TR" => LabeledEdgeSide.TR,
            "R" => LabeledEdgeSide.R,
            "BR" => LabeledEdgeSide.BR,
            "B" => LabeledEdgeSide.B,
            "BL" => LabeledEdgeSide.BL,
            "L" => LabeledEdgeSide.L,
            _ => default,
        };

        return raw is "TL" or "T" or "TR" or "R" or "BR" or "B" or "BL" or "L";
    }

    static bool TryParseLabeledEdgesScope(string raw, out LabeledEdgesScope scope)
    {
        scope = raw switch
        {
            "Desktop" => LabeledEdgesScope.Desktop,
            "Output" => LabeledEdgesScope.Output,
            "Slice" => LabeledEdgesScope.Slice,
            _ => default,
        };

        return raw is "Desktop" or "Output" or "Slice";
    }

    static LogoOverride? ParseLogoOverride(TomlTable parent, string key, string configDirectory)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new LogoOverride
        {
            Source = ResolveConfigRelativePath(GetString(t, "source"), configDirectory),
            X = GetString(t, "x"),
            Y = GetString(t, "y"),
            AnchorX = GetString(t, "anchor-x") ?? GetString(t, "anchorX"),
            AnchorY = GetString(t, "anchor-y") ?? GetString(t, "anchorY"),
            Width = GetString(t, "width"),
            Height = GetString(t, "height"),
            Opacity = GetFloatRequiredRange(t, "opacity", $"config [{key}].opacity", minInclusive: 0f, maxInclusive: 1f),
        };
    }

    static ImmutableArray<string>? ResolveConfigRelativePathArray(ImmutableArray<string>? values, string configDirectory)
    {
        return ConfiguredPathResolver.ResolveArray(values, configDirectory);
    }

    static string ResolveConfigRelativePath(string? value, string configDirectory)
    {
        return ConfiguredPathResolver.Resolve(value, configDirectory);
    }

    static ImmutableArray<string>? GetStringArray(TomlTable t, string key)
    {
        if (!t.TryGetValue(key, out object? obj)) return null;
        if (obj is TomlArray arr) return [.. arr.OfType<string>()];
        if (obj is string s) return [s];
        return null;
    }

    static ImmutableArray<float>? GetFloatArrayRequiredRange(
        TomlTable t,
        string key,
        string source,
        float minInclusive,
        float maxInclusive)
    {
        if (!t.TryGetValue(key, out object? obj))
            return null;

        if (obj is TomlArray arr)
        {
            ImmutableArray<float>.Builder values = ImmutableArray.CreateBuilder<float>(arr.Count);
            for (int i = 0; i < arr.Count; i++)
            {
                if (!TryConvertTomlNumber(arr[i], out float parsed))
                    throw new FormatException($"Invalid numeric value in {source}[{i}]: '{arr[i]}'; expected a float.");
                ValidateRange(parsed, source, minInclusive, maxInclusive);
                values.Add(parsed);
            }
            return values.ToImmutable();
        }

        if (!TryConvertTomlNumber(obj, out float single))
            throw new FormatException($"Invalid numeric value in {source}: '{obj}'; expected a float or float array.");

        ValidateRange(single, source, minInclusive, maxInclusive);
        return [single];
    }

    static float? GetFloatRequiredRange(TomlTable t, string key, string source, float minInclusive, float maxInclusive)
    {
        if (!t.TryGetValue(key, out object? obj))
            return null;

        if (!TryConvertTomlNumber(obj, out float value))
            throw new FormatException($"Invalid numeric value in {source}: '{obj}'; expected a float.");

        ValidateRange(value, source, minInclusive, maxInclusive);
        return value;
    }

    static bool TryConvertTomlNumber(object? value, out float parsed)
    {
        switch (value)
        {
            case double d:
                parsed = (float)d;
                return true;
            case long l:
                parsed = l;
                return true;
            case int i:
                parsed = i;
                return true;
            case float f:
                parsed = f;
                return true;
            default:
                parsed = 0f;
                return false;
        }
    }

    static void ValidateRange(float value, string source, float minInclusive, float maxInclusive)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new FormatException(
                $"Invalid numeric value {value.ToString(System.Globalization.CultureInfo.InvariantCulture)} in {source}; expected value in range [{minInclusive.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {maxInclusive.ToString(System.Globalization.CultureInfo.InvariantCulture)}].");
        }
    }

    static ImmutableArray<bool>? GetBoolArray(TomlTable t, string key)
    {
        if (!t.TryGetValue(key, out object? obj)) return null;
        if (obj is TomlArray arr)
            return [.. arr.Select(item => item is bool b && b)];
        if (obj is bool b2) return [b2];
        return null;
    }

    static string? GetString(TomlTable t, string key) =>
        t.TryGetValue(key, out object? obj) && obj is string s ? s : null;

    static bool? GetBool(TomlTable t, string key) =>
        t.TryGetValue(key, out object? obj) && obj is bool b ? b : null;

    static int? GetInt(TomlTable t, string key) =>
        t.TryGetValue(key, out object? obj) && TryConvertTomlNumber(obj, out float parsed) ? (int)Math.Round(parsed) : null;

    static LogLevel ParseLogLevel(string? raw, string source, List<string>? warnings)
    {
        return raw?.Trim().ToLowerInvariant() switch
        {
            "quiet" => LogLevel.Warning,
            "normal" or "" or null => LogLevel.Information,
            "verbose" => LogLevel.Debug,
            _ => Fallback(raw, source, warnings),
        };

        static LogLevel Fallback(string raw, string source, List<string>? warnings)
        {
            warnings?.Add($"Invalid verbosity '{raw}' from {source}; falling back to 'normal'.");
            return LogLevel.Information;
        }
    }
}