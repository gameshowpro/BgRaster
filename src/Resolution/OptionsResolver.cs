// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Resolution;

internal static class OptionsResolver
{
    private static readonly ImmutableArray<string> s_implicitFullOutputSliceDefaultText =
    [
        "${MachineName} ${OutputIndex}",
        "${OutputName}",
        "${OutputWidth}x${OutputHeight}",
    ];

    internal static ResolvedOptions Resolve(GlobalOptions global, OutputRecord output, OutputOptions? outputConfig, int systemWidthPx = 0, int systemHeightPx = 0)
    {
        int idx = output.Index;
        float vw = output.WidthPx;
        float vh = output.HeightPx;
        string machineName = ResolveMachineName(global.Render.MachineName);
        NetworkOptions network = MergeNetwork(global.Network, outputConfig?.Network);
        ImmutableArray<AdapterInfo> networkAdapters = NetworkFilter.Apply(
            global.Render.SimulateNetwork
                ? NetworkSimulator.GetAdapters()
                : NetworkCollector.Collect(),
            network);

        SubstitutionContext ctx = new(
            MachineName: machineName,
            OutputWidth: output.WidthPx,
            OutputHeight: output.HeightPx,
            OutputIndex: idx,
            OutputName: output.FriendlyName,
            SliceWidth: output.WidthPx,
            SliceHeight: output.HeightPx);
        string currentDirectory = Directory.GetCurrentDirectory();

        ImmutableArray<string> textLines = [.. ResolveTextArray(global.Text.Format, outputConfig?.Text?.Format).Select(line => Substitute(line, ctx))];
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
        string textTextAlign = outputConfig?.Text?.TextAlign ?? global.Text.TextAlign;
        string textAnchorX = outputConfig?.Text?.AnchorX ?? global.Text.AnchorX;
        string textAnchorY = outputConfig?.Text?.AnchorY ?? global.Text.AnchorY;
        float textXPx = ParseUnit(ResolveString(global.Text.X, idx, outputConfig?.Text?.X), vw, vh);
        float textYPx = ParseUnit(ResolveString(global.Text.Y, idx, outputConfig?.Text?.Y), vw, vh);

        SKColor bgColor = ParseColor(ResolveString(global.Background.Color, idx, outputConfig?.Background?.Color));
        string bgImage = ConfiguredPathResolver.Resolve(
            ResolveString(global.Background.Image, idx, outputConfig?.Background?.Image),
            currentDirectory,
            ctx);
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

        float circleXPx = ParseUnit(ResolveString(global.Circle.X, idx, outputConfig?.Circle?.X), vw, vh);
        float circleYPx = ParseUnit(ResolveString(global.Circle.Y, idx, outputConfig?.Circle?.Y), vw, vh);
        float circleSizePx = ParseUnit(ResolveString(global.Circle.Size, idx, outputConfig?.Circle?.Size), vw, vh);
        SKColor circleColor = ParseColor(ResolveString(global.Circle.Color, idx, outputConfig?.Circle?.Color));
        float circleStrokePx = ParseUnit(ResolveString(global.Circle.Stroke, idx, outputConfig?.Circle?.Stroke), vw, vh);

        float crosshairXPx = ParseUnit(ResolveString(global.Crosshair.X, idx, outputConfig?.Crosshair?.X), vw, vh);
        float crosshairYPx = ParseUnit(ResolveString(global.Crosshair.Y, idx, outputConfig?.Crosshair?.Y), vw, vh);
        float crosshairLengthPx = ParseUnit(ResolveString(global.Crosshair.Length, idx, outputConfig?.Crosshair?.Length), vw, vh);
        SKColor crosshairColor = ParseColor(ResolveString(global.Crosshair.Color, idx, outputConfig?.Crosshair?.Color));
        float crosshairStrokePx = ParseUnit(ResolveString(global.Crosshair.Stroke, idx, outputConfig?.Crosshair?.Stroke), vw, vh);

        ImmutableArray<LabeledEdgeSide> labeledEdgesSides = outputConfig?.LabeledEdges?.Side ?? global.LabeledEdges.Side;
        LabeledEdgesScope labeledEdgesScope = ResolveLabeledEdgesScope(global.LabeledEdges.Scope, idx, outputConfig?.LabeledEdges?.Scope);
        (float scopeWidthPx, float scopeHeightPx) = ResolveLabeledEdgesScopeDimensions(labeledEdgesScope, output.WidthPx, output.HeightPx, output.WidthPx, output.HeightPx, systemWidthPx, systemHeightPx);
        float labeledEdgesTextSizePx = ParseUnit(ResolveString(global.LabeledEdges.TextSize, idx, outputConfig?.LabeledEdges?.TextSize), scopeWidthPx, scopeHeightPx);
        float labeledEdgesTailLengthPx = ParseUnit(ResolveString(global.LabeledEdges.TailLength, idx, outputConfig?.LabeledEdges?.TailLength), scopeWidthPx, scopeHeightPx);
        float labeledEdgesThicknessPx = ParseUnit(ResolveString(global.LabeledEdges.Thickness, idx, outputConfig?.LabeledEdges?.Thickness), scopeWidthPx, scopeHeightPx);
        float labeledEdgesHeadScale = ResolveFloat(global.LabeledEdges.HeadScale, idx, outputConfig?.LabeledEdges?.HeadScale);

        string logoSource = ConfiguredPathResolver.Resolve(
            ResolveString(global.Logo.Source, idx, outputConfig?.Logo?.Source),
            currentDirectory,
            ctx);
        string logoAnchorX = outputConfig?.Logo?.AnchorX ?? global.Logo.AnchorX;
        string logoAnchorY = outputConfig?.Logo?.AnchorY ?? global.Logo.AnchorY;
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
            TextTextAlign = textTextAlign,
            TextAnchorX = textAnchorX,
            TextAnchorY = textAnchorY,
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
            CircleXPx = circleXPx,
            CircleYPx = circleYPx,
            CircleSizePx = circleSizePx,
            CircleColor = circleColor,
            CircleStrokePx = circleStrokePx,
            CrosshairXPx = crosshairXPx,
            CrosshairYPx = crosshairYPx,
            CrosshairLengthPx = crosshairLengthPx,
            CrosshairColor = crosshairColor,
            CrosshairStrokePx = crosshairStrokePx,
            LabeledEdgesTextSizePx = labeledEdgesTextSizePx,
            LabeledEdgesTailLengthPx = labeledEdgesTailLengthPx,
            LabeledEdgesThicknessPx = labeledEdgesThicknessPx,
            LabeledEdgesHeadScale = labeledEdgesHeadScale,
            LabeledEdgesScope = labeledEdgesScope,
            LabeledEdgesScopeWidthPx = scopeWidthPx,
            LabeledEdgesScopeHeightPx = scopeHeightPx,
            LabeledEdgesSides = labeledEdgesSides,
            LogoSource = logoSource,
            LogoAnchorX = logoAnchorX,
            LogoAnchorY = logoAnchorY,
            LogoXPx = logoXPx,
            LogoYPx = logoYPx,
            LogoWidthPx = logoWidthPx,
            LogoHeightPx = logoHeightPx,
            LogoOpacity = logoOpacity,
            NetworkAdapters = networkAdapters,
            NetworkOptions = network,
            NetworkSizesPx = ResolveSizedLines(network.Size, network.Size.Length, vw, vh),
            NetworkColors = ResolveColorLines(network.Color, network.Color.Length),
            NetworkTextAlign = network.TextAlign,
            NetworkAnchorX = network.AnchorX,
            NetworkAnchorY = network.AnchorY,
            NetworkXPx = ParseUnit(ResolveString(network.X, idx, null), vw, vh),
            NetworkYPx = ParseUnit(ResolveString(network.Y, idx, null), vw, vh),
        };
    }

    internal static ResolvedOptions ResolveForSlice(
        GlobalOptions global, OutputRecord output, OutputOptions? outputConfig,
        SliceOptions slice, int sliceWidth, int sliceHeight, int? sequenceIndex = null, int sliceIndex = 0, bool isImplicitSlice = false,
        int systemWidthPx = 0, int systemHeightPx = 0)
    {
        int idx = output.Index;
        int cycleIndex = sequenceIndex ?? idx;
        float vw = sliceWidth;
        float vh = sliceHeight;
        string machineName = ResolveMachineName(global.Render.MachineName);
        NetworkOptions network = MergeNetwork(global.Network, outputConfig?.Network, slice.Network);
        ImmutableArray<AdapterInfo> networkAdapters = NetworkFilter.Apply(
            global.Render.SimulateNetwork
                ? NetworkSimulator.GetAdapters()
                : NetworkCollector.Collect(),
            network);

        SubstitutionContext ctx = new(
            MachineName: machineName,
            OutputWidth: output.WidthPx,
            OutputHeight: output.HeightPx,
            OutputIndex: idx,
            OutputName: output.FriendlyName,
            SliceWidth: sliceWidth,
            SliceHeight: sliceHeight,
            SliceIndex: sliceIndex);
        string currentDirectory = Directory.GetCurrentDirectory();

        ImmutableArray<string> textLines = [.. ResolveSliceTextArray(
                global.Text.Format,
                outputConfig?.Text?.Format,
                slice.Text?.Format,
                isImplicitSlice)
            .Select(line => Substitute(line, ctx))];
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
        string textTextAlign = slice.Text?.TextAlign ?? outputConfig?.Text?.TextAlign ?? global.Text.TextAlign;
        string textAnchorX = slice.Text?.AnchorX ?? outputConfig?.Text?.AnchorX ?? global.Text.AnchorX;
        string textAnchorY = slice.Text?.AnchorY ?? outputConfig?.Text?.AnchorY ?? global.Text.AnchorY;
        float textXPx = ParseUnit(ResolveSliceString(global.Text.X, cycleIndex, outputConfig?.Text?.X, slice.Text?.X), vw, vh);
        float textYPx = ParseUnit(ResolveSliceString(global.Text.Y, cycleIndex, outputConfig?.Text?.Y, slice.Text?.Y), vw, vh);

        SKColor bgColor = ParseColor(ResolveSliceString(global.Background.Color, cycleIndex, outputConfig?.Background?.Color, slice.Background?.Color));
        string bgImage = ConfiguredPathResolver.Resolve(
            ResolveSliceString(global.Background.Image, cycleIndex, outputConfig?.Background?.Image, slice.Background?.Image),
            currentDirectory,
            ctx);
        FitMode bgFit = ParseFitMode(ResolveSliceString(global.Background.Fit, cycleIndex, outputConfig?.Background?.Fit, slice.Background?.Fit));
        bool alternating = ResolveSliceBool(global.Background.Alternating, cycleIndex, outputConfig?.Background?.Alternating, slice.Background?.Alternating);
        bool border = ResolveSliceBool(global.Background.Border, cycleIndex, outputConfig?.Background?.Border, slice.Background?.Border);
        SKColor borderColor = ParseColor(ResolveSliceString(global.Background.BorderColor, cycleIndex, outputConfig?.Background?.BorderColor, slice.Background?.BorderColor));

        float gridSizePx = ParseUnit(ResolveSliceString(global.Grid.Size, cycleIndex, outputConfig?.Grid?.Size, slice.Grid?.Size), vw, vh);
        SKColor gridOddColor = ParseColor(ResolveSliceString(global.Grid.OddColor, cycleIndex, outputConfig?.Grid?.OddColor, slice.Grid?.OddColor));
        SKColor gridEvenColor = ParseColor(ResolveSliceString(global.Grid.EvenColor, cycleIndex, outputConfig?.Grid?.EvenColor, slice.Grid?.EvenColor));
        float gridStrokePx = ParseUnit(ResolveSliceString(global.Grid.Stroke, cycleIndex, outputConfig?.Grid?.Stroke, slice.Grid?.Stroke), vw, vh);
        float gridOffsetXPx = ParseUnit(ResolveSliceString(global.Grid.OffsetX, cycleIndex, outputConfig?.Grid?.OffsetX, slice.Grid?.OffsetX), vw, vh);
        float gridOffsetYPx = ParseUnit(ResolveSliceString(global.Grid.OffsetY, cycleIndex, outputConfig?.Grid?.OffsetY, slice.Grid?.OffsetY), vw, vh);
        bool gridCoordinates = ResolveSliceBool(global.Grid.Coordinates, cycleIndex, outputConfig?.Grid?.Coordinates, slice.Grid?.Coordinates);

        float circleXPx = ParseUnit(ResolveSliceString(global.Circle.X, cycleIndex, outputConfig?.Circle?.X, slice.Circle?.X), vw, vh);
        float circleYPx = ParseUnit(ResolveSliceString(global.Circle.Y, cycleIndex, outputConfig?.Circle?.Y, slice.Circle?.Y), vw, vh);
        float circleSizePx = ParseUnit(ResolveSliceString(global.Circle.Size, cycleIndex, outputConfig?.Circle?.Size, slice.Circle?.Size), vw, vh);
        SKColor circleColor = ParseColor(ResolveSliceString(global.Circle.Color, cycleIndex, outputConfig?.Circle?.Color, slice.Circle?.Color));
        float circleStrokePx = ParseUnit(ResolveSliceString(global.Circle.Stroke, cycleIndex, outputConfig?.Circle?.Stroke, slice.Circle?.Stroke), vw, vh);

        float crosshairXPx = ParseUnit(ResolveSliceString(global.Crosshair.X, cycleIndex, outputConfig?.Crosshair?.X, slice.Crosshair?.X), vw, vh);
        float crosshairYPx = ParseUnit(ResolveSliceString(global.Crosshair.Y, cycleIndex, outputConfig?.Crosshair?.Y, slice.Crosshair?.Y), vw, vh);
        float crosshairLengthPx = ParseUnit(ResolveSliceString(global.Crosshair.Length, cycleIndex, outputConfig?.Crosshair?.Length, slice.Crosshair?.Length), vw, vh);
        SKColor crosshairColor = ParseColor(ResolveSliceString(global.Crosshair.Color, cycleIndex, outputConfig?.Crosshair?.Color, slice.Crosshair?.Color));
        float crosshairStrokePx = ParseUnit(ResolveSliceString(global.Crosshair.Stroke, cycleIndex, outputConfig?.Crosshair?.Stroke, slice.Crosshair?.Stroke), vw, vh);

        ImmutableArray<LabeledEdgeSide> labeledEdgesSides = slice.LabeledEdges?.Side ?? outputConfig?.LabeledEdges?.Side ?? global.LabeledEdges.Side;
        LabeledEdgesScope labeledEdgesScope = ResolveLabeledEdgesScope(global.LabeledEdges.Scope, cycleIndex, outputConfig?.LabeledEdges?.Scope, slice.LabeledEdges?.Scope);
        (float scopeWidthPx, float scopeHeightPx) = ResolveLabeledEdgesScopeDimensions(labeledEdgesScope, output.WidthPx, output.HeightPx, sliceWidth, sliceHeight, systemWidthPx, systemHeightPx);
        float labeledEdgesTextSizePx = ParseUnit(ResolveSliceString(global.LabeledEdges.TextSize, cycleIndex, outputConfig?.LabeledEdges?.TextSize, slice.LabeledEdges?.TextSize), scopeWidthPx, scopeHeightPx);
        float labeledEdgesTailLengthPx = ParseUnit(ResolveSliceString(global.LabeledEdges.TailLength, cycleIndex, outputConfig?.LabeledEdges?.TailLength, slice.LabeledEdges?.TailLength), scopeWidthPx, scopeHeightPx);
        float labeledEdgesThicknessPx = ParseUnit(ResolveSliceString(global.LabeledEdges.Thickness, cycleIndex, outputConfig?.LabeledEdges?.Thickness, slice.LabeledEdges?.Thickness), scopeWidthPx, scopeHeightPx);
        float labeledEdgesHeadScale = ResolveSliceFloat(global.LabeledEdges.HeadScale, cycleIndex, outputConfig?.LabeledEdges?.HeadScale, slice.LabeledEdges?.HeadScale);

        string logoSource = ConfiguredPathResolver.Resolve(
            ResolveSliceString(global.Logo.Source, cycleIndex, outputConfig?.Logo?.Source, slice.Logo?.Source),
            currentDirectory,
            ctx);
        string logoAnchorX = slice.Logo?.AnchorX ?? outputConfig?.Logo?.AnchorX ?? global.Logo.AnchorX;
        string logoAnchorY = slice.Logo?.AnchorY ?? outputConfig?.Logo?.AnchorY ?? global.Logo.AnchorY;
        float logoXPx = ParseUnit(ResolveSliceString(global.Logo.X, cycleIndex, outputConfig?.Logo?.X, slice.Logo?.X), vw, vh);
        float logoYPx = ParseUnit(ResolveSliceString(global.Logo.Y, cycleIndex, outputConfig?.Logo?.Y, slice.Logo?.Y), vw, vh);
        float logoWidthPx = ParseUnit(ResolveSliceString(global.Logo.Width, cycleIndex, outputConfig?.Logo?.Width, slice.Logo?.Width), vw, vh);
        float logoHeightPx = ParseUnit(ResolveSliceString(global.Logo.Height, cycleIndex, outputConfig?.Logo?.Height, slice.Logo?.Height), vw, vh);
        float logoOpacity = ResolveSliceFloat(global.Logo.Opacity, cycleIndex, outputConfig?.Logo?.Opacity, slice.Logo?.Opacity);

        return new ResolvedOptions
        {
            TextLines = textLines,
            TextSizesPx = textSizesPx,
            TextColors = textColors,
            TextTextAlign = textTextAlign,
            TextAnchorX = textAnchorX,
            TextAnchorY = textAnchorY,
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
            CircleXPx = circleXPx,
            CircleYPx = circleYPx,
            CircleSizePx = circleSizePx,
            CircleColor = circleColor,
            CircleStrokePx = circleStrokePx,
            CrosshairXPx = crosshairXPx,
            CrosshairYPx = crosshairYPx,
            CrosshairLengthPx = crosshairLengthPx,
            CrosshairColor = crosshairColor,
            CrosshairStrokePx = crosshairStrokePx,
            LabeledEdgesTextSizePx = labeledEdgesTextSizePx,
            LabeledEdgesTailLengthPx = labeledEdgesTailLengthPx,
            LabeledEdgesThicknessPx = labeledEdgesThicknessPx,
            LabeledEdgesHeadScale = labeledEdgesHeadScale,
            LabeledEdgesScope = labeledEdgesScope,
            LabeledEdgesScopeWidthPx = scopeWidthPx,
            LabeledEdgesScopeHeightPx = scopeHeightPx,
            LabeledEdgesSides = labeledEdgesSides,
            LogoSource = logoSource,
            LogoAnchorX = logoAnchorX,
            LogoAnchorY = logoAnchorY,
            LogoXPx = logoXPx,
            LogoYPx = logoYPx,
            LogoWidthPx = logoWidthPx,
            LogoHeightPx = logoHeightPx,
            LogoOpacity = logoOpacity,
            NetworkAdapters = networkAdapters,
            NetworkOptions = network,
            NetworkSizesPx = ResolveSizedLines(network.Size, network.Size.Length, vw, vh),
            NetworkColors = ResolveColorLines(network.Color, network.Color.Length),
            NetworkTextAlign = network.TextAlign,
            NetworkAnchorX = network.AnchorX,
            NetworkAnchorY = network.AnchorY,
            NetworkXPx = ParseUnit(ResolveSliceString(network.X, cycleIndex, null, null), vw, vh),
            NetworkYPx = ParseUnit(ResolveSliceString(network.Y, cycleIndex, null, null), vw, vh),
        };
    }

    private static string ResolveString(ImmutableArray<string> global, int index, string? outputOverride)
    {
        if (outputOverride is not null)
        {
            return outputOverride;
        }

        if (global.IsDefaultOrEmpty)
        {
            return "";
        }

        return global[index % global.Length];
    }

    private static string ResolveMachineName(string configuredMachineName) =>
        string.IsNullOrWhiteSpace(configuredMachineName) ? Environment.MachineName : configuredMachineName;

    private static string ResolveSliceString(ImmutableArray<string> global, int index, string? outputOverride, string? sliceOverride)
    {
        if (sliceOverride is not null)
        {
            return sliceOverride;
        }

        if (outputOverride is not null)
        {
            return outputOverride;
        }

        if (global.IsDefaultOrEmpty)
        {
            return "";
        }

        return global[index % global.Length];
    }

    private static float ResolveFloat(ImmutableArray<float> global, int index, float? outputOverride)
    {
        if (outputOverride is not null)
        {
            return outputOverride.Value;
        }

        if (global.IsDefaultOrEmpty)
        {
            return 0f;
        }

        return global[index % global.Length];
    }

    private static float ResolveSliceFloat(ImmutableArray<float> global, int index, float? outputOverride, float? sliceOverride)
    {
        if (sliceOverride is not null)
        {
            return sliceOverride.Value;
        }

        if (outputOverride is not null)
        {
            return outputOverride.Value;
        }

        if (global.IsDefaultOrEmpty)
        {
            return 0f;
        }

        return global[index % global.Length];
    }

    private static LabeledEdgesScope ResolveLabeledEdgesScope(ImmutableArray<LabeledEdgesScope> global, int index, string? outputOverride)
    {
        if (outputOverride is not null && TryParseLabeledEdgesScope(outputOverride, out LabeledEdgesScope scope))
        {
            return scope;
        }

        return global.Length == 0 ? LabeledEdgesScope.Output : global[index % global.Length];
    }

    private static LabeledEdgesScope ResolveLabeledEdgesScope(ImmutableArray<LabeledEdgesScope> global, int index, string? outputOverride, string? sliceOverride)
    {
        if (sliceOverride is not null && TryParseLabeledEdgesScope(sliceOverride, out LabeledEdgesScope sliceScope))
        {
            return sliceScope;
        }

        if (outputOverride is not null && TryParseLabeledEdgesScope(outputOverride, out LabeledEdgesScope outputScope))
        {
            return outputScope;
        }

        return global.Length == 0 ? LabeledEdgesScope.Output : global[index % global.Length];
    }

    private static (float WidthPx, float HeightPx) ResolveLabeledEdgesScopeDimensions(
        LabeledEdgesScope scope,
        int outputWidthPx,
        int outputHeightPx,
        int sliceWidthPx,
        int sliceHeightPx,
        int systemWidthPx,
        int systemHeightPx) => scope switch
        {
            LabeledEdgesScope.Desktop => (
                systemWidthPx > 0 ? systemWidthPx : outputWidthPx,
                systemHeightPx > 0 ? systemHeightPx : outputHeightPx),
            LabeledEdgesScope.Slice => (
                sliceWidthPx > 0 ? sliceWidthPx : outputWidthPx,
                sliceHeightPx > 0 ? sliceHeightPx : outputHeightPx),
            _ => (outputWidthPx, outputHeightPx),
        };

    private static bool TryParseLabeledEdgesScope(string raw, out LabeledEdgesScope scope)
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

    private static ImmutableArray<string> ResolveTextArray(ImmutableArray<string> global, ImmutableArray<string>? outputOverride)
    {
        if (outputOverride is { Length: > 0 })
        {
            return outputOverride.Value;
        }

        return global.Length == 0 ? [""] : global;
    }

    private static ImmutableArray<string> ResolveSliceTextArray(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride,
        bool isImplicitSlice)
    {
        if (sliceOverride is { Length: > 0 })
        {
            return sliceOverride.Value;
        }

        if (outputOverride is { Length: > 0 })
        {
            return outputOverride.Value;
        }

        if (isImplicitSlice && IsDefaultSliceText(global))
        {
            return s_implicitFullOutputSliceDefaultText;
        }

        return global.Length == 0 ? [""] : global;
    }

    private static bool IsDefaultSliceText(ImmutableArray<string> textValues) =>
        textValues.SequenceEqual(new TextOptions().Format);

    private static ImmutableArray<float> ResolveTextSizes(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        int lineCount,
        float vw,
        float vh)
    {
        ImmutableArray<string> source = ResolveTextArray(global, outputOverride);
        return ResolveSizedLines(source, lineCount, vw, vh);
    }

    private static ImmutableArray<float> ResolveSliceTextSizes(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride,
        int lineCount,
        float vw,
        float vh)
    {
        ImmutableArray<string> source = ResolveSliceTextArray(global, outputOverride, sliceOverride, isImplicitSlice: false);
        return ResolveSizedLines(source, lineCount, vw, vh);
    }

    private static ImmutableArray<float> ResolveSizedLines(ImmutableArray<string> sizeValues, int lineCount, float vw, float vh)
    {
        if (lineCount <= 0)
        {
            return [];
        }

        ImmutableArray<string> source = sizeValues.Length == 0 ? ["0"] : sizeValues;
        ImmutableArray<float>.Builder result = ImmutableArray.CreateBuilder<float>(lineCount);
        for (int i = 0; i < lineCount; i++)
        {
            result.Add(ParseUnit(source[i % source.Length], vw, vh));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<SKColor> ResolveTextColors(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        int lineCount)
    {
        ImmutableArray<string> source = ResolveTextArray(global, outputOverride);
        return ResolveColorLines(source, lineCount);
    }

    private static ImmutableArray<SKColor> ResolveSliceTextColors(
        ImmutableArray<string> global,
        ImmutableArray<string>? outputOverride,
        ImmutableArray<string>? sliceOverride,
        int lineCount)
    {
        ImmutableArray<string> source = ResolveSliceTextArray(global, outputOverride, sliceOverride, isImplicitSlice: false);
        return ResolveColorLines(source, lineCount);
    }

    private static ImmutableArray<SKColor> ResolveColorLines(ImmutableArray<string> colorValues, int lineCount)
    {
        if (lineCount <= 0)
        {
            return [];
        }

        ImmutableArray<string> source = colorValues.Length == 0 ? ["#fff"] : colorValues;
        ImmutableArray<SKColor>.Builder result = ImmutableArray.CreateBuilder<SKColor>(lineCount);
        for (int i = 0; i < lineCount; i++)
        {
            result.Add(ParseTextColor(source[i % source.Length]));
        }
        return result.ToImmutable();
    }

    private static bool ResolveBool(ImmutableArray<bool> global, int index, bool? outputOverride) =>
        outputOverride ?? global[index % global.Length];

    private static bool ResolveSliceBool(ImmutableArray<bool> global, int index, bool? outputOverride, bool? sliceOverride) =>
        sliceOverride ?? outputOverride ?? global[index % global.Length];

    private static float ParseUnit(string value, float vw, float vh)
    {
        try
        { return UnitParser.Parse(value).ResolvePixels(vw, vh); }
        catch (FormatException) { return 0f; }
    }

    private static SKColor ParseColor(string value)
    {
        if (ColorParser.TryParse(value, out SKColor color))
        {
            return color;
        }

        return SKColors.Transparent;
    }

    private static SKColor ParseTextColor(string value)
    {
        if (ColorParser.TryParse(value, out SKColor color))
        {
            return color;
        }

        return SKColors.White;
    }

    private static FitMode ParseFitMode(string value)
    {
        try
        { return FitModeParser.Parse(value); }
        catch (FormatException) { return FitMode.CropToFill; }
    }

    private static string Substitute(string template, SubstitutionContext ctx) =>
        FieldSubstitutor.Substitute(template, ctx);

    private static NetworkOptions MergeNetwork(NetworkOptions global, NetworkOverride? outputOverride, NetworkOverride? sliceOverride = null)
    {
        NetworkOptions result = global;
        if (outputOverride is not null)
        {
            result = MergeOne(result, outputOverride);
        }

        if (sliceOverride is not null)
        {
            result = MergeOne(result, sliceOverride);
        }

        return result;
    }

    private static NetworkOptions MergeOne(NetworkOptions base_, NetworkOverride override_) =>
        base_ with
        {
            RequireAdapterType = override_.RequireAdapterType ?? base_.RequireAdapterType,
            ExcludeAdapterType = override_.ExcludeAdapterType ?? base_.ExcludeAdapterType,
            RequireUp = override_.RequireUp ?? base_.RequireUp,
            RequireFamily = override_.RequireFamily ?? base_.RequireFamily,
            RequireMacAddress = override_.RequireMacAddress ?? base_.RequireMacAddress,
            RequireSubnet = override_.RequireSubnet ?? base_.RequireSubnet,
            MinimumAddressCount = override_.MinimumAddressCount ?? base_.MinimumAddressCount,
            RequireName = override_.RequireName ?? base_.RequireName,
            RequireDescription = override_.RequireDescription ?? base_.RequireDescription,
            IpAddressFormat = override_.IpAddressFormat ?? base_.IpAddressFormat,
            AdapterFormat = override_.AdapterFormat ?? base_.AdapterFormat,
            TextAlign = override_.TextAlign ?? base_.TextAlign,
            AnchorX = override_.AnchorX ?? base_.AnchorX,
            AnchorY = override_.AnchorY ?? base_.AnchorY,
            X = override_.X ?? base_.X,
            Y = override_.Y ?? base_.Y,
            Size = override_.Size ?? base_.Size,
            Color = override_.Color ?? base_.Color,
            Render = override_.Render ?? base_.Render,
        };
}
