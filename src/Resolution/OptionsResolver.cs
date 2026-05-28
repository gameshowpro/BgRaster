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

        string title = Substitute(ResolveString(global.Text.Title, idx, outputConfig?.Text?.Title), ctx);
        string subtitle = Substitute(ResolveString(global.Text.Subtitle, idx, outputConfig?.Text?.Subtitle), ctx);
        float textSizePx = ParseUnit(ResolveString(global.Text.Size, idx, outputConfig?.Text?.Size), vw, vh);
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
        float logoOpacity = ParseOpacity(ResolveString(global.Logo.Opacity, idx, outputConfig?.Logo?.Opacity));

        return new ResolvedOptions
        {
            Title = title,
            Subtitle = subtitle,
            TextSizePx = textSizePx,
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

        string title = Substitute(ResolveSliceString(global.Text.Title, idx, outputConfig?.Text?.Title, slice.Text?.Title), ctx);
        string subtitle = Substitute(ResolveSliceString(global.Text.Subtitle, idx, outputConfig?.Text?.Subtitle, slice.Text?.Subtitle), ctx);
        float textSizePx = ParseUnit(ResolveSliceString(global.Text.Size, idx, outputConfig?.Text?.Size, slice.Text?.Size), vw, vh);
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
        float logoOpacity = ParseOpacity(ResolveSliceString(global.Logo.Opacity, idx, outputConfig?.Logo?.Opacity, slice.Logo?.Opacity));

        return new ResolvedOptions
        {
            Title = title,
            Subtitle = subtitle,
            TextSizePx = textSizePx,
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

    static FitMode ParseFitMode(string value)
    {
        try { return FitModeParser.Parse(value); }
        catch (FormatException) { return FitMode.CropToFill; }
    }

    static float ParseOpacity(string value)
    {
        if (float.TryParse(value.Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float f))
            return Math.Clamp(f, 0f, 1f);
        return 1f;
    }

    static string Substitute(string template, SubstitutionContext ctx) =>
        FieldSubstitutor.Substitute(template, ctx);
}
