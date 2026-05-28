namespace GameshowPro.BgRaster.Configuration;

static class ConfigLoader
{
    internal static GlobalOptions Load(string path, List<string>? warnings = null)
    {
        string toml = File.ReadAllText(path);
        TomlTable table = Toml.ToModel(toml);
        return ParseGlobalOptions(table, warnings);
    }

    internal static GlobalOptions ApplyCliOverlay(GlobalOptions options, CliOverlay overlay, List<string>? warnings = null)
    {
        TextOptions text = options.Text;
        BackgroundOptions bg = options.Background;
        GridOptions grid = options.Grid;
        CircleOptions circle = options.Circle;
        CrosshairOptions crosshair = options.Crosshair;
        LogoOptions logo = options.Logo;
        RenderOptions render = options.Render;

        if (overlay.Text is not null)
            text = text with { Text = ParseCliStringOrArray(overlay.Text, "cli --text", warnings) };
        if (overlay.TextSize is not null)
            text = text with { Size = ParseCliStringOrArray(overlay.TextSize, "cli --text-size", warnings) };
        if (overlay.TextColor is not null)
            text = text with { Color = ParseCliStringOrArray(overlay.TextColor, "cli --text-color", warnings) };
        if (overlay.TextX is not null)
            text = text with { X = ParseCliStringOrArray(overlay.TextX, "cli --text-x", warnings) };
        if (overlay.TextY is not null)
            text = text with { Y = ParseCliStringOrArray(overlay.TextY, "cli --text-y", warnings) };

        if (overlay.BackgroundColor is not null)
            bg = bg with { Color = ParseCliStringOrArray(overlay.BackgroundColor, "cli --background-color", warnings) };
        if (overlay.BackgroundImage is not null)
            bg = bg with { Image = ParseCliStringOrArray(overlay.BackgroundImage, "cli --background-image", warnings) };
        if (overlay.BackgroundFit is not null)
            bg = bg with { Fit = ParseCliStringOrArray(overlay.BackgroundFit, "cli --background-fit", warnings) };
        if (overlay.BackgroundAlternating is not null) bg = bg with { Alternating = [overlay.BackgroundAlternating.Value] };
        if (overlay.BackgroundBorder is not null) bg = bg with { Border = [overlay.BackgroundBorder.Value] };
        if (overlay.BackgroundBorderColor is not null)
            bg = bg with { BorderColor = ParseCliStringOrArray(overlay.BackgroundBorderColor, "cli --background-border-color", warnings) };

        if (overlay.GridSize is not null)
            grid = grid with { Size = ParseCliStringOrArray(overlay.GridSize, "cli --grid-size", warnings) };
        if (overlay.GridOddColor is not null)
            grid = grid with { OddColor = ParseCliStringOrArray(overlay.GridOddColor, "cli --grid-odd-color", warnings) };
        if (overlay.GridEvenColor is not null)
            grid = grid with { EvenColor = ParseCliStringOrArray(overlay.GridEvenColor, "cli --grid-even-color", warnings) };
        if (overlay.GridStroke is not null)
            grid = grid with { Stroke = ParseCliStringOrArray(overlay.GridStroke, "cli --grid-stroke", warnings) };
        if (overlay.GridOffsetX is not null)
            grid = grid with { OffsetX = ParseCliStringOrArray(overlay.GridOffsetX, "cli --grid-offset-x", warnings) };
        if (overlay.GridOffsetY is not null)
            grid = grid with { OffsetY = ParseCliStringOrArray(overlay.GridOffsetY, "cli --grid-offset-y", warnings) };
        if (overlay.GridCoordinates is not null) grid = grid with { Coordinates = [overlay.GridCoordinates.Value] };

        if (overlay.CircleSize is not null)
            circle = circle with { Size = ParseCliStringOrArray(overlay.CircleSize, "cli --circle-size", warnings) };
        if (overlay.CircleColor is not null)
            circle = circle with { Color = ParseCliStringOrArray(overlay.CircleColor, "cli --circle-color", warnings) };
        if (overlay.CircleStroke is not null)
            circle = circle with { Stroke = ParseCliStringOrArray(overlay.CircleStroke, "cli --circle-stroke", warnings) };

        if (overlay.CrosshairLength is not null)
            crosshair = crosshair with { Length = ParseCliStringOrArray(overlay.CrosshairLength, "cli --crosshair-length", warnings) };
        if (overlay.CrosshairColor is not null)
            crosshair = crosshair with { Color = ParseCliStringOrArray(overlay.CrosshairColor, "cli --crosshair-color", warnings) };
        if (overlay.CrosshairStroke is not null)
            crosshair = crosshair with { Stroke = ParseCliStringOrArray(overlay.CrosshairStroke, "cli --crosshair-stroke", warnings) };

        if (overlay.LogoSource is not null)
            logo = logo with { Source = ParseCliStringOrArray(overlay.LogoSource, "cli --logo-source", warnings) };
        if (overlay.LogoX is not null)
            logo = logo with { X = ParseCliStringOrArray(overlay.LogoX, "cli --logo-x", warnings) };
        if (overlay.LogoY is not null)
            logo = logo with { Y = ParseCliStringOrArray(overlay.LogoY, "cli --logo-y", warnings) };
        if (overlay.LogoWidth is not null)
            logo = logo with { Width = ParseCliStringOrArray(overlay.LogoWidth, "cli --logo-width", warnings) };
        if (overlay.LogoHeight is not null)
            logo = logo with { Height = ParseCliStringOrArray(overlay.LogoHeight, "cli --logo-height", warnings) };
        if (overlay.LogoOpacity is not null)
            logo = logo with { Opacity = ParseCliFloatOrArray(overlay.LogoOpacity, "cli --logo-opacity") };

        if (overlay.RenderDryRun is not null) render = render with { DryRun = overlay.RenderDryRun.Value };
        if (overlay.RenderOutputsSkipUnspecified is not null)
            render = render with { OutputsSkipUnspecified = overlay.RenderOutputsSkipUnspecified.Value };
        if (overlay.RenderOutput is not null) render = render with { Output = overlay.RenderOutput };
        if (overlay.RenderVerbosity is not null)
        {
            render = render with
            {
                MinimumLogLevel = ParseLogLevel(
                    overlay.RenderVerbosity,
                    "cli --verbosity",
                    warnings),
            };
        }
        if (overlay.RenderContinueAfterUnchanged is not null)
            render = render with { ContinueAfterUnchanged = overlay.RenderContinueAfterUnchanged.Value };

        return options with
        {
            Text = text,
            Background = bg,
            Grid = grid,
            Circle = circle,
            Crosshair = crosshair,
            Logo = logo,
            Render = render,
        };
    }

    static GlobalOptions ParseGlobalOptions(TomlTable table, List<string>? warnings)
    {
        TextOptions text = table.TryGetValue("text", out object? textObj) && textObj is TomlTable textTable
            ? ParseTextOptions(textTable)
            : new TextOptions();

        BackgroundOptions background = table.TryGetValue("background", out object? bgObj) && bgObj is TomlTable bgTable
            ? ParseBackgroundOptions(bgTable)
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

        LogoOptions logo = table.TryGetValue("logo", out object? logoObj) && logoObj is TomlTable logoTable
            ? ParseLogoOptions(logoTable)
            : new LogoOptions();

        RenderOptions render = table.TryGetValue("render", out object? renderObj) && renderObj is TomlTable renderTable
            ? ParseRenderOptions(renderTable, warnings)
            : new RenderOptions();

        ImmutableArray<OutputOptions> outputs = table.TryGetValue("output", out object? outputObj) && outputObj is TomlTableArray outputArray
            ? [.. outputArray.Select(ParseOutputOptions)]
            : [];

        return new GlobalOptions
        {
            Text = text,
            Background = background,
            Grid = grid,
            Circle = circle,
            Crosshair = crosshair,
            Logo = logo,
            Render = render,
            Outputs = outputs,
        };
    }

    static TextOptions ParseTextOptions(TomlTable t) => new()
    {
        Text = GetStringArray(t, "text") ?? ParseLegacyText(t) ?? new TextOptions().Text,
        Size = GetStringArray(t, "size") ?? new TextOptions().Size,
        Color = GetStringArray(t, "color") ?? new TextOptions().Color,
        X = GetStringArray(t, "x") ?? new TextOptions().X,
        Y = GetStringArray(t, "y") ?? new TextOptions().Y,
    };

    static BackgroundOptions ParseBackgroundOptions(TomlTable t) => new()
    {
        Color = GetStringArray(t, "color") ?? new BackgroundOptions().Color,
        Image = GetStringArray(t, "image") ?? new BackgroundOptions().Image,
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
        Size = GetStringArray(t, "size") ?? new CircleOptions().Size,
        Color = GetStringArray(t, "color") ?? new CircleOptions().Color,
        Stroke = GetStringArray(t, "stroke") ?? new CircleOptions().Stroke,
    };

    static CrosshairOptions ParseCrosshairOptions(TomlTable t) => new()
    {
        Length = GetStringArray(t, "length") ?? new CrosshairOptions().Length,
        Color = GetStringArray(t, "color") ?? new CrosshairOptions().Color,
        Stroke = GetStringArray(t, "stroke") ?? new CrosshairOptions().Stroke,
    };

    static LogoOptions ParseLogoOptions(TomlTable t) => new()
    {
        Source = GetStringArray(t, "source") ?? new LogoOptions().Source,
        X = GetStringArray(t, "x") ?? new LogoOptions().X,
        Y = GetStringArray(t, "y") ?? new LogoOptions().Y,
        Width = GetStringArray(t, "width") ?? new LogoOptions().Width,
        Height = GetStringArray(t, "height") ?? new LogoOptions().Height,
        Opacity = GetFloatArrayRequiredRange(t, "opacity", "config [logo].opacity", minInclusive: 0f, maxInclusive: 1f) ?? new LogoOptions().Opacity,
    };

    static RenderOptions ParseRenderOptions(TomlTable t, List<string>? warnings)
    {
        string? verbosityText = GetString(t, "verbosity");

        return new RenderOptions
        {
            DryRun = GetBool(t, "no-assignment") ?? false,
            OutputsSkipUnspecified = GetBool(t, "outputs-skip-unspecified") ?? false,
            Output = GetString(t, "output") ?? "",
            MinimumLogLevel = ParseLogLevel(verbosityText, "config [render].verbosity", warnings),
            ContinueAfterUnchanged = GetBool(t, "force") ?? GetBool(t, "render-force") ?? false,
        };
    }

    static OutputOptions ParseOutputOptions(TomlTable t)
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
            ? [.. sliceArray.Select(ParseSliceOptions)]
            : [];

        return new OutputOptions
        {
            Target = target,
            Text = ParseTextOverride(t, "text"),
            Background = ParseBackgroundOverride(t, "background"),
            Grid = ParseGridOverride(t, "grid"),
            Circle = ParseCircleOverride(t, "circle"),
            Crosshair = ParseCrosshairOverride(t, "crosshair"),
            Logo = ParseLogoOverride(t, "logo"),
            Slices = slices,
        };
    }

    static SliceOptions ParseSliceOptions(TomlTable t) => new()
    {
        X = GetString(t, "x") ?? "0",
        Y = GetString(t, "y") ?? "0",
        Width = GetString(t, "width") ?? "100vw",
        Height = GetString(t, "height") ?? "100vh",
        Text = ParseTextOverride(t, "text"),
        Background = ParseBackgroundOverride(t, "background"),
        Grid = ParseGridOverride(t, "grid"),
        Circle = ParseCircleOverride(t, "circle"),
        Crosshair = ParseCrosshairOverride(t, "crosshair"),
        Logo = ParseLogoOverride(t, "logo"),
    };

    static TextOverride? ParseTextOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;

        ImmutableArray<string>? textOverride = GetStringArray(t, "text") ?? ParseLegacyText(t);

        return new TextOverride
        {
            Text = textOverride,
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

        string[] defaults = new TextOptions().Text.ToArray();
        defaults[0] = title is { Length: > 0 } ? title.Value[0] : defaults[0];
        defaults[2] = subtitle is { Length: > 0 } ? subtitle.Value[0] : defaults[2];
        return [.. defaults];
    }

    static ImmutableArray<string> ParseCliStringOrArray(string raw, string source, List<string>? warnings)
    {
        string trimmed = raw.Trim();
        if (!trimmed.StartsWith("[", StringComparison.Ordinal))
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

    static ImmutableArray<float> ParseCliFloatOrArray(string raw, string source)
    {
        string trimmed = raw.Trim();
        if (!trimmed.StartsWith("[", StringComparison.Ordinal))
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

    static BackgroundOverride? ParseBackgroundOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new BackgroundOverride
        {
            Color = GetString(t, "color"),
            Image = GetString(t, "image"),
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
            Length = GetString(t, "length"),
            Color = GetString(t, "color"),
            Stroke = GetString(t, "stroke"),
        };
    }

    static LogoOverride? ParseLogoOverride(TomlTable parent, string key)
    {
        if (!parent.TryGetValue(key, out object? obj) || obj is not TomlTable t) return null;
        return new LogoOverride
        {
            Source = GetString(t, "source"),
            X = GetString(t, "x"),
            Y = GetString(t, "y"),
            Width = GetString(t, "width"),
            Height = GetString(t, "height"),
            Opacity = GetFloatRequiredRange(t, "opacity", $"config [{key}].opacity", minInclusive: 0f, maxInclusive: 1f),
        };
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
            return [.. arr.Select(item => item is bool b ? b : false)];
        if (obj is bool b2) return [b2];
        return null;
    }

    static string? GetString(TomlTable t, string key) =>
        t.TryGetValue(key, out object? obj) && obj is string s ? s : null;

    static bool? GetBool(TomlTable t, string key) =>
        t.TryGetValue(key, out object? obj) && obj is bool b ? b : null;

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
