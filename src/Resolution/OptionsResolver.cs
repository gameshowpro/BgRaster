namespace GameshowPro.BgRaster.Resolution;

static class OptionsResolver
{
    internal static ResolvedOptions Resolve(GlobalOptions global, OutputRecord output, OutputOptions? outputConfig)
    {
        int idx = output.Index;
        float vw = output.WidthPx;
        float vh = output.HeightPx;

        SubstitutionContext ctx = new(
            MachineName: Environment.MachineName,
            Width: output.WidthPx,
            Height: output.HeightPx,
            Index: idx,
            OutputName: output.FriendlyName);

        ImmutableArray<string> textLines = ResolveTextArray(global.Text.Text, outputConfig?.Text?.Text)
            .Select(line => Substitute(line, ctx))
            .ToImmutableArray();
        ImmutableArray<float> textSizesPx = ResolveTextSizes(
            global.Text.Size,
            outputConfig?.Text?.Size,
            textLines.Length,
            vw,
            vh);
        ImmutableArray<SKColor> textColors = ResolveTextColors(
            global.Text.Color,
            outputConfig?.Text?.Color,
            textLines.Length);
        float textXPx = ParseUnit(ResolveString(global.Text.X, idx, outputConfig?.Text?.X), vw, vh);
        float textYPx = ParseUnit(ResolveString(global.Text.Y, idx, outputConfig?.Text?.Y), vw, vh);

        SKColor bgColor = ParseColor(ResolveString(global.Background.Color, idx, outputConfig?.Background?.Color));
        string bgImage = ResolveString(global.Background.Image, idx, outputConfig?.Background?.Image);
        FitMode bgFit = ParseFitMode(ResolveString(global.Background.Fit, idx, outputConfig?.Background?.Fit));
        bool alternating = ResolveBool(global.Background.Alternating, idx, outputConfig?.Background?.Alternating);
        bool border = ResolveBool(global.Background.Border, idx, outputConfig?.Background?.Border);
        SKColor borderColor = ParseColor(ResolveString(global.Background.BorderColor, idx, outputConfig?.Background?.BorderColor));

        float gridSizePx = ParseUnit(ResolveString(global.Grid.Size, idx, outputConfig?.Grid?.Size), vw, vh);
        SKColor gridOddColor = ParseColor(ResolveString(global.Grid.OddColor, idx, outputConfig?.Grid?.OddColor));
        SKColor gridEvenColor = ParseColor(ResolveString(global.Grid.EvenColor, idx, outputConfig?.Grid?.EvenColor));
        float gridStrokePx = ParseUnit(ResolveString(global.Grid.Stroke, idx, outputConfig?.Grid?.Stroke), vw, vh);
        float gridOffsetXPx = ParseUnit(ResolveString(global.Grid.OffsetX, idx, outputConfig?.Grid?.OffsetX), vw, vh);
        float gridOffsetYPx = ParseUnit(ResolveString(global.Grid.OffsetY, idx, outputConfig?.Grid?.OffsetY), vw, vh);
        bool gridCoordinates = ResolveBool(global.Grid.Coordinates, idx, outputConfig?.Grid?.Coordinates);

        float circleSizePx = ParseUnit(ResolveString(global.Circle.Size, idx, outputConfig?.Circle?.Size), vw, vh);
        SKColor circleColor = ParseColor(ResolveString(global.Circle.Color, idx, outputConfig?.Circle?.Color));
        float circleStrokePx = ParseUnit(ResolveString(global.Circle.Stroke, idx, outputConfig?.Circle?.Stroke), vw, vh);

        float crosshairLengthPx = ParseUnit(ResolveString(global.Crosshair.Length, idx, outputConfig?.Crosshair?.Length), vw, vh);
        SKColor crosshairColor = ParseColor(ResolveString(global.Crosshair.Color, idx, outputConfig?.Crosshair?.Color));
        float crosshairStrokePx = ParseUnit(ResolveString(global.Crosshair.Stroke, idx, outputConfig?.Crosshair?.Stroke), vw, vh);

        string logoSource = ResolveString(global.Logo.Source, idx, outputConfig?.Logo?.Source);
        float logoXPx = ParseUnit(ResolveString(global.Logo.X, idx, outputConfig?.Logo?.X), vw, vh);
        float logoYPx = ParseUnit(ResolveString(global.Logo.Y, idx, outputConfig?.Logo?.Y), vw, vh);
        float logoWidthPx = ParseUnit(ResolveString(global.Logo.Width, idx, outputConfig?.Logo?.Width), vw, vh);
        float logoHeightPx = ParseUnit(ResolveString(global.Logo.Height, idx, outputConfig?.Logo?.Height), vw, vh);
        float logoOpacity = ResolveFloat(global.Logo.Opacity, idx, outputConfig?.Logo?.Opacity);

        return new ResolvedOptions
        {
            TextLines = textLines,
            TextSizesPx = textSizesPx,
            TextColors = textColors,
            TextXPx = textXPx,
            TextYPx = textYPx,
            BackgroundColor = bgColor,
            BackgroundImage = bgImage,
            BackgroundFit = bgFit,
            Alternating = alternating,
            Border = border,
            BorderColor = borderColor,
            GridSizePx = gridSizePx,
            GridOddColor = gridOddColor,
            GridEvenColor = gridEvenColor,
            GridStrokePx = gridStrokePx,
            GridOffsetXPx = gridOffsetXPx,
            GridOffsetYPx = gridOffsetYPx,
            GridCoordinates = gridCoordinates,
            CircleSizePx = circleSizePx,
            CircleColor = circleColor,
            CircleStrokePx = circleStrokePx,
            CrosshairLengthPx = crosshairLengthPx,
            CrosshairColor = crosshairColor,
            CrosshairStrokePx = crosshairStrokePx,
            LogoSource = logoSource,
            LogoXPx = logoXPx,
            LogoYPx = logoYPx,
            LogoWidthPx = logoWidthPx,
            LogoHeightPx = logoHeightPx,
            LogoOpacity = logoOpacity,
        };
    }

    internal static ResolvedOptions ResolveForSlice(
        GlobalOptions global, OutputRecord output, OutputOptions? outputConfig,
        SliceOptions slice, int sliceWidth, int sliceHeight)
    {
        int idx = output.Index;
        float vw = sliceWidth;
        float vh = sliceHeight;

        SubstitutionContext ctx = new(
            MachineName: Environment.MachineName,
            Width: sliceWidth,
            Height: sliceHeight,
            Index: idx,
            OutputName: output.FriendlyName,
            ParentIndex: idx);

        ImmutableArray<string> textLines = ResolveSliceTextArray(
                global.Text.Text,
                outputConfig?.Text?.Text,
                slice.Text?.Text)
            .Select(line => Substitute(line, ctx))
            .ToImmutableArray();
        ImmutableArray<float> textSizesPx = ResolveSliceTextSizes(
            global.Text.Size,
            outputConfig?.Text?.Size,
            slice.Text?.Size,
            textLines.Length,
            vw,
            vh);
        ImmutableArray<SKColor> textColors = ResolveSliceTextColors(
            global.Text.Color,
            outputConfig?.Text?.Color,
            slice.Text?.Color,
            textLines.Length);
        float textXPx = ParseUnit(ResolveSliceString(global.Text.X, idx, outputConfig?.Text?.X, slice.Text?.X), vw, vh);
        float textYPx = ParseUnit(ResolveSliceString(global.Text.Y, idx, outputConfig?.Text?.Y, slice.Text?.Y), vw, vh);

        SKColor bgColor = ParseColor(ResolveSliceString(global.Background.Color, idx, outputConfig?.Background?.Color, slice.Background?.Color));
        string bgImage = ResolveSliceString(global.Background.Image, idx, outputConfig?.Background?.Image, slice.Background?.Image);
        FitMode bgFit = ParseFitMode(ResolveSliceString(global.Background.Fit, idx, outputConfig?.Background?.Fit, slice.Background?.Fit));
        bool alternating = ResolveSliceBool(global.Background.Alternating, idx, outputConfig?.Background?.Alternating, slice.Background?.Alternating);
        bool border = ResolveSliceBool(global.Background.Border, idx, outputConfig?.Background?.Border, slice.Background?.Border);
        SKColor borderColor = ParseColor(ResolveSliceString(global.Background.BorderColor, idx, outputConfig?.Background?.BorderColor, slice.Background?.BorderColor));

        float gridSizePx = ParseUnit(ResolveSliceString(global.Grid.Size, idx, outputConfig?.Grid?.Size, slice.Grid?.Size), vw, vh);
        SKColor gridOddColor = ParseColor(ResolveSliceString(global.Grid.OddColor, idx, outputConfig?.Grid?.OddColor, slice.Grid?.OddColor));
        SKColor gridEvenColor = ParseColor(ResolveSliceString(global.Grid.EvenColor, idx, outputConfig?.Grid?.EvenColor, slice.Grid?.EvenColor));
        float gridStrokePx = ParseUnit(ResolveSliceString(global.Grid.Stroke, idx, outputConfig?.Grid?.Stroke, slice.Grid?.Stroke), vw, vh);
        float gridOffsetXPx = ParseUnit(ResolveSliceString(global.Grid.OffsetX, idx, outputConfig?.Grid?.OffsetX, slice.Grid?.OffsetX), vw, vh);
        float gridOffsetYPx = ParseUnit(ResolveSliceString(global.Grid.OffsetY, idx, outputConfig?.Grid?.OffsetY, slice.Grid?.OffsetY), vw, vh);
        bool gridCoordinates = ResolveSliceBool(global.Grid.Coordinates, idx, outputConfig?.Grid?.Coordinates, slice.Grid?.Coordinates);

        float circleSizePx = ParseUnit(ResolveSliceString(global.Circle.Size, idx, outputConfig?.Circle?.Size, slice.Circle?.Size), vw, vh);
        SKColor circleColor = ParseColor(ResolveSliceString(global.Circle.Color, idx, outputConfig?.Circle?.Color, slice.Circle?.Color));
        float circleStrokePx = ParseUnit(ResolveSliceString(global.Circle.Stroke, idx, outputConfig?.Circle?.Stroke, slice.Circle?.Stroke), vw, vh);

        float crosshairLengthPx = ParseUnit(ResolveSliceString(global.Crosshair.Length, idx, outputConfig?.Crosshair?.Length, slice.Crosshair?.Length), vw, vh);
        SKColor crosshairColor = ParseColor(ResolveSliceString(global.Crosshair.Color, idx, outputConfig?.Crosshair?.Color, slice.Crosshair?.Color));
        float crosshairStrokePx = ParseUnit(ResolveSliceString(global.Crosshair.Stroke, idx, outputConfig?.Crosshair?.Stroke, slice.Crosshair?.Stroke), vw, vh);

        string logoSource = ResolveSliceString(global.Logo.Source, idx, outputConfig?.Logo?.Source, slice.Logo?.Source);
        float logoXPx = ParseUnit(ResolveSliceString(global.Logo.X, idx, outputConfig?.Logo?.X, slice.Logo?.X), vw, vh);
        float logoYPx = ParseUnit(ResolveSliceString(global.Logo.Y, idx, outputConfig?.Logo?.Y, slice.Logo?.Y), vw, vh);
        float logoWidthPx = ParseUnit(ResolveSliceString(global.Logo.Width, idx, outputConfig?.Logo?.Width, slice.Logo?.Width), vw, vh);
        float logoHeightPx = ParseUnit(ResolveSliceString(global.Logo.Height, idx, outputConfig?.Logo?.Height, slice.Logo?.Height), vw, vh);
        float logoOpacity = ResolveSliceFloat(global.Logo.Opacity, idx, outputConfig?.Logo?.Opacity, slice.Logo?.Opacity);

        return new ResolvedOptions
        {
            TextLines = textLines,
            TextSizesPx = textSizesPx,
            TextColors = textColors,
            TextXPx = textXPx,
            TextYPx = textYPx,
            BackgroundColor = bgColor,
            BackgroundImage = bgImage,
            BackgroundFit = bgFit,
            Alternating = alternating,
            Border = border,
            BorderColor = borderColor,
            GridSizePx = gridSizePx,
            GridOddColor = gridOddColor,
            GridEvenColor = gridEvenColor,
            GridStrokePx = gridStrokePx,
            GridOffsetXPx = gridOffsetXPx,
            GridOffsetYPx = gridOffsetYPx,
            GridCoordinates = gridCoordinates,
            CircleSizePx = circleSizePx,
            CircleColor = circleColor,
            CircleStrokePx = circleStrokePx,
            CrosshairLengthPx = crosshairLengthPx,
            CrosshairColor = crosshairColor,
            CrosshairStrokePx = crosshairStrokePx,
            LogoSource = logoSource,
            LogoXPx = logoXPx,
            LogoYPx = logoYPx,
            LogoWidthPx = logoWidthPx,
            LogoHeightPx = logoHeightPx,
            LogoOpacity = logoOpacity,
        };
    }

    static string ResolveString(ImmutableArray<string> global, int index, string? outputOverride) =>
        outputOverride ?? global[index % global.Length];

    static string ResolveSliceString(ImmutableArray<string> global, int index, string? outputOverride, string? sliceOverride) =>
        sliceOverride ?? outputOverride ?? global[index % global.Length];

    static float ResolveFloat(ImmutableArray<float> global, int index, float? outputOverride) =>
        outputOverride ?? global[index % global.Length];

    static float ResolveSliceFloat(ImmutableArray<float> global, int index, float? outputOverride, float? sliceOverride) =>
        sliceOverride ?? outputOverride ?? global[index % global.Length];

    static ImmutableArray<string> ResolveTextArray(ImmutableArray<string> global, ImmutableArray<string>? outputOverride)
    {
        if (outputOverride is { Length: > 0 })
            return outputOverride.Value;

        return global.Length == 0 ? [""] : global;
    }

    static ImmutableArray<string> ResolveSliceTextArray(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride)
    {
        if (sliceOverride is { Length: > 0 })
            return sliceOverride.Value;
        if (outputOverride is { Length: > 0 })
            return outputOverride.Value;

        return global.Length == 0 ? [""] : global;
    }

    static ImmutableArray<float> ResolveTextSizes(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        int lineCount,
        float vw,
        float vh)
    {
        ImmutableArray<string> source = ResolveTextArray(global, outputOverride);
        return ResolveSizedLines(source, lineCount, vw, vh);
    }

    static ImmutableArray<float> ResolveSliceTextSizes(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride,
        int lineCount,
        float vw,
        float vh)
    {
        ImmutableArray<string> source = ResolveSliceTextArray(global, outputOverride, sliceOverride);
        return ResolveSizedLines(source, lineCount, vw, vh);
    }

    static ImmutableArray<float> ResolveSizedLines(ImmutableArray<string> sizeValues, int lineCount, float vw, float vh)
    {
        if (lineCount <= 0)
            return [];

        ImmutableArray<string> source = sizeValues.Length == 0 ? ["0"] : sizeValues;
        ImmutableArray<float>.Builder result = ImmutableArray.CreateBuilder<float>(lineCount);
        for (int i = 0; i < lineCount; i++)
        {
            result.Add(ParseUnit(source[i % source.Length], vw, vh));
        }
        return result.ToImmutable();
    }

    static ImmutableArray<SKColor> ResolveTextColors(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        int lineCount)
    {
        ImmutableArray<string> source = ResolveTextArray(global, outputOverride);
        return ResolveColorLines(source, lineCount);
    }

    static ImmutableArray<SKColor> ResolveSliceTextColors(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride,
        int lineCount)
    {
        ImmutableArray<string> source = ResolveSliceTextArray(global, outputOverride, sliceOverride);
        return ResolveColorLines(source, lineCount);
    }

    static ImmutableArray<SKColor> ResolveColorLines(ImmutableArray<string> colorValues, int lineCount)
    {
        if (lineCount <= 0)
            return [];

        ImmutableArray<string> source = colorValues.Length == 0 ? ["#fff"] : colorValues;
        ImmutableArray<SKColor>.Builder result = ImmutableArray.CreateBuilder<SKColor>(lineCount);
        for (int i = 0; i < lineCount; i++)
        {
            result.Add(ParseTextColor(source[i % source.Length]));
        }
        return result.ToImmutable();
    }

    static bool ResolveBool(ImmutableArray<bool> global, int index, bool? outputOverride) =>
        outputOverride ?? global[index % global.Length];

    static bool ResolveSliceBool(ImmutableArray<bool> global, int index, bool? outputOverride, bool? sliceOverride) =>
        sliceOverride ?? outputOverride ?? global[index % global.Length];

    static float ParseUnit(string value, float vw, float vh)
    {
        try { return UnitParser.Parse(value).ResolvePixels(vw, vh); }
        catch (FormatException) { return 0f; }
    }

    static SKColor ParseColor(string value)
    {
        if (ColorParser.TryParse(value, out SKColor color)) return color;
        return SKColors.Transparent;
    }

    static SKColor ParseTextColor(string value)
    {
        if (ColorParser.TryParse(value, out SKColor color)) return color;
        return SKColors.White;
    }

    static FitMode ParseFitMode(string value)
    {
        try { return FitModeParser.Parse(value); }
        catch (FormatException) { return FitMode.CropToFill; }
    }

    static string Substitute(string template, SubstitutionContext ctx) =>
        FieldSubstitutor.Substitute(template, ctx);
}
